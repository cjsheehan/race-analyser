#! /usr/bin/perl -w

########################################################################################################
# file          : xmlHorseBuilder.pl                                                                   #
# contents      : Takes horse data in xml format and parses this data and generates a spreadsheet with #
#				          relevant formulas and predictions                                            #
########################################################################################################
# History:                                                                                             #
#               xmlHorseBuilder v0.1 : 25/05/2011                                                      #
#                                     First Release                                                    #
#               xmlHorseBuilder v0.2 : 26/05/2011                                                      #
#                                     1. Fixed bug with formulas that use VLOOKUP                      #
#                                     2. Created various extra formats to cover                        #
#                                        data types float/int/str                                      #
#               xmlHorseBuilder v0.3 : 26/05/2011                                                      #
#                                     1. Added functionality for getting the previous beaten distance  #
#               xmlHorseBuilder v0.4 : 28/05/2011                                                      #
#                                     1. Fixed functionality fBor getting the previous beaten distance #
#                                        Now uses previous psoition as index for tallying up           # 
#                                        beaten distance                                               #
#                                     2. Changed get* functions to return -1 instead of 0 when         # 
#                                        unsuccessful                                                  #
#                                     3. Fixed actSpeedFormula to use actDistance rather than Track    # 
#                                        distance                                                      #
#               xmlHorseBuilder v0.5 : 02/06/2011                                                      #
#                                     1. Fixed bug where a distance that was of format Xm Xy didn't    #
#                                        have the yards added to the total                             #
#               xmlHorseBuilder v0.6 : 22/07/2011                                                      #
#                                     1. Fixed bug where a distance that was of format Xf Xy didn't    #
#                                        have the yards added to the total                             #
#               xmlHorseBuilder v0.7 : 29/02/2012                                                      #
#                                     1. Added bCompletedRace and conditional checks to ensure that    #
#                                        a fallen horse or unseated rider has the appropriate info in  # 
#                                        the beaten distance box                                       # 
#               xmlHorseBuilder v0.8 : 29/02/2012                                                      #
#               xmlHorseBuilder v1.0 : 12/02/2014                                                      #
#                                     1. Simplified writeRaceInfo                                      #   
#                                     2. Fixed extraction of Course and Date for naming spreadsheet    # 
#                                     3. Added prize money info for today's race and horse's previous  # 
#                                        races.  As a result I had to reuse the Jockey Time column.    #
#                                     4. Support DateTime xml node including extracting time and date  #
#                                        individually via formatDateTime();                            #
#                                     5. Worksheets are always now in chronological order              #
#                                     6. Refactored WriteHorseInfo for writing beaten distances        #
#                                        /positions and formats.                                       #
#                                     7. Fixed 'no history' horses from having red flags against       #
#                                        missing going.                                                #
#                                     8. Missing wintime is now red flagged                            #
#                                     9. Going 'good, good to soft in places' - fixed incorrect val    #
#               xmlHorseBuilder v1.1 : 14/09/2014                                                      #
#                                     1. XML reading now creates a new data structure :   %g_allDates  #
#                                        Date->Meeting->Time, this allows multiple races with different#
#                                        dates and meetings. This impacted all subs who were writing to#
#                                        the current sheet with details from the structure.            #
#               xmlHorseBuilder v1.2: 24/08/2015                                                       #
#                                     1. Excel::Writer::XLSX is now used for writing spreadsheets.     #
#                                        It supports modern .xlsx formats and better handling of       #
#                                        formulas.                                                     #
#               xmlHorseBuilder v1.3: 06/03/17                                                         #
#                                     1. Handle new XML data from scraper                              #
#                                     1. Bugfix : going support "soft-.*"                              #
#               xmlHorseBuilder v1.4: 06/11/17                                                         #
#                                     1. Write extra Race class info                                   #
#                                     2. Runner info columns now accessed via column defs              #
#               xmlHorseBuilder v1.5: 21/11/17                                                         #
#                                     1. Write todays race win prize money                             #
#                                     2. Bugfix: race time < min now writes correctly                  #
########################################################################################################

use strict;
use Excel::Writer::XLSX;
use XML::Simple;
use Readonly;
use Data::Dumper;
use File::Path qw(make_path);
use Spreadsheet::ParseExcel::Utility 'col2int';
      
########################################################################################################
# Check arguments & options & global vars                                                              #
########################################################################################################
my $loc = $ARGV[1];
my $sheetsLoc = $loc . "/";
my $version = "v1.5";
my $debug = 0;
my $debugFH;
my $debugFile = $loc  . "xmlHorseDEBUG.txt";
my %g_allDates;
my $g_Size = 0;

if ($debug)
{
  open($debugFH, '>', $debugFile) or die "Can't open $debugFile for write: $!";
}

########################################################################################################
# Spreadsheet formats                                                                                  #
########################################################################################################
my ($headFormat1, $headFormat2, $entryFormatRight, $strFormatOrangeCB, $entryFormatUrl,
    $strFormatGreenCB, $intFormatGreenCB, $floatFormatGreenCB,
    $strFormatRedCB, $intFormatRedCB, $floatFormatRedCB,
  $strFormatSilverCB, $intFormatSilverCB, $floatFormatSilverCB,
  $strFormatLimeCB, $intFormatLimeCB, $floatFormatLimeCB,
    $strFormatCB, $floatFormatCB, $intFormatCB, $entryFormat1,
  $entryFormatLeft, $float4FormatLimeCB, $entryFormatUrlOrangeBg, $entryFormatUrlRedBg,
  $strFormatHLCB, $intFormatHLCB, $floatFormatHLCB);
  

########################################################################################################
# Spreadsheet Formulas                                                                                 #
########################################################################################################
 
my $infoStartRow = 10;

########################################################################################################
# XML Keys                                                                                             #
########################################################################################################

my $RACE = 'Race';
my $COURSE = 'Course';
my $DATE = 'Date';
my $TIME = 'Time';
my $TITLE = 'Title';
my $URL = 'Url';
my $RACE_TYPE = 'Type';
my $IS_FLAT_RACE = 'Flat';
my $DISTANCE = 'Distance';
my $GOING = 'Going';
my $CATEGORY = 'Category';
my $WIN_PRIZE = 'WinPrize';
my $NUM_RUNNERS = 'NumberOfRunners';
my $ENTRANTS = 'Entrants';
my $ENTRANT = 'Entrant';
my $TRAINER_NAME = 'TrainerName';
my $TRAINER_URL = 'TrainerUrl';;
my $JOCKEY_NAME = 'JockeyName';
my $JOCKEY_CLAIM = 'JockeyClaim';
my $JOCKEY_URL = 'JockeyUrl';
my $HORSE_NAME = 'HorseName';
my $HORSE_URL = 'HorseUrl';
my $SADDLE_NUM = 'SaddleNumber';
my $STALL_NUM = 'StallNumber';
my $AGE = 'Age';
my $FORM = 'Form';
my $FORM_WATCH = 'FormWatch';
my $LAST_RAN = 'LastRan';
my $WEIGHT = 'Weight';
my $ODDS = 'Odds';
my $LAST_RACE = 'LastRace';
my $PRIZE = 'LastRacePrizes';
my $WINNING_TIME = 'WinningTime';
my $BEATEN_LENGTHS = 'BeatenLengths';
my $ANALYSIS = 'Analysis';
my $CLASS = 'Class';
my $RATING = 'Rating';
my $POSITION = 'Position';
my $INFO = 'Info';

########################################################################################################
# Column Defs                                                                                          #
########################################################################################################

my %cols = (
    horse => "A",
    pred_time => "B",
    pred_adj_time => "C",
    prev_pos => "D",
    form => "E",
    since_last => "F",
    trainer => "G",
    jockey => "H",
    todays_weight => "I",
    prev_weight => "J",
    prev_prize => "K",
    prev_class => "L",
    prev_distance_str => "M",
    prev_distance_yds => "N",
    prev_beaten_lengths => "O",
    actual_prev_distance => "P",
    winning_time_str => "Q",
    winning_time_sec => "R",
    prev_going_str=> "S",
    prev_going_ordinal => "T",
    avg_yds_per_sec => "U",
    actual_yds_per_sec => "V",
    diff_yds_per_sec => "W",
    pred_yds_per_sec => "X"
);

########################################################################################################
#ImportXml                                                                                             #
########################################################################################################
my $parser = new XML::Simple;
my $data = $parser->XMLin($ARGV[0]); # initialize parser and read the file
	
if ($debug)
{
  print $debugFH Dumper( $data );	
}

########################################################################################################
# Populate the main race data structure                                                                #
########################################################################################################

if(ref($data->{$RACE}) eq "ARRAY")
{
  $g_Size = $#{ $data->{$RACE} };
}

for my $i (0..$g_Size)
{
  my $race;
  if($g_Size == 0)
  {
    $race = $data->{$RACE};
  }
  else
  {
    $race = $data->{$RACE}[$i];
  }
  
  my $date = $race->{$DATE}; 
  my $course = $race->{$COURSE};
  push @{ $g_allDates{$date}{$course} }, $race;
}

########################################################################################################
# Setup global vars for race data (dependent on whether there is one or more races
# as the xml parser will create an array for > 1 and a scalar for == 1                                 #
########################################################################################################
if ($debug)
{
  open($debugFH, '>', $debugFile) or die "Can't open $debugFile for write: $!";
}

########################################################################################################
#main code                                                                                             #
########################################################################################################

my $g_activeRace;
my $g_jumps;

foreach my $date (keys %g_allDates)
{
  # Check if folder exists, if not create it
  my $destDir = $sheetsLoc . $date;
  make_path($destDir);
  
  foreach my $course ( keys %{ $g_allDates{$date} } )
  {
    # Write::Excell cannot append, only write new
    my $bookFilePath = $destDir . "/" . $course . ".xlsx";    
    my $dest_book  = Excel::Writer::XLSX->new("$bookFilePath") or die "Could not create a new Excel file in $bookFilePath: $!";
    addFormats(\$dest_book);
    
    foreach my $race (@{ $g_allDates{$date}{$course} })
    {

      # Check if sheet exists, if not create it          
      my $sheet = $race->{$TIME} . "_" . $course;
      $sheet=~ s/[\:\s+]/_/g; # Replace unsupported characters with underscore
      my $dest_sheet = $dest_book->add_worksheet($sheet); # Add new sheet

      # Write the sheet data
      $g_jumps = 0;
      if($race->{$RACE_TYPE} ne 'FLAT')
      {
        $g_jumps = 1;
      }
      
      $g_activeRace = $race;
      #createDynamicFormulas(\$dest_sheet);
      setCellDimension(\$dest_sheet);
      writeRaceHeading(\$dest_sheet);
      writeRaceInfo(\$dest_sheet);
      writeLookupTable(\$dest_sheet);
      writeHorseHeadings(\$dest_sheet);
      writeHorseInfo(\$dest_sheet);
      writeVersionInfo(\$dest_sheet, $version);
    }
    
  } 
}


########################################################################################################
# subroutines                                                                                          #
########################################################################################################

sub addDynamicFormulas {
  print "addDynamicFormulas\n" if $debug;
  my $dest_sheet_ref = shift;
  my $raceSize = shift;
  my $row = 11;
   
  for my $iHorses ( 0..$raceSize ) 
  {    
    my $form_idx = $row + 1;
    my $actYdsFormula = "=IF($cols{prev_beaten_lengths}$form_idx >= 0, $cols{prev_distance_yds}$form_idx - ($cols{prev_beaten_lengths}$form_idx*2.6), 0)";
    my $actSpeedFormula = "=IF($cols{winning_time_sec}$form_idx >= 0, $cols{actual_prev_distance}$form_idx/$cols{winning_time_sec}$form_idx,0)";
    my $speedDiffFormula = "=(100/$cols{avg_yds_per_sec}$form_idx)*($cols{actual_yds_per_sec}$form_idx - $cols{avg_yds_per_sec}$form_idx)";
    my $predSpeedFormula = "=D6+(D6*($cols{diff_yds_per_sec}$form_idx/100))";
    my $predTimeFormula = "=C5/$cols{pred_yds_per_sec}$form_idx";
    my $jockeyTimeFormula  = "=$cols{pred_time}$form_idx + G$form_idx";
    my $sec2TimeFormula  = "=(((C5-$cols{prev_distance_yds}$form_idx)/220)*2)+$cols{pred_time}$form_idx";
    
    $$dest_sheet_ref->write_formula($row, col2int($cols{actual_prev_distance}), $actYdsFormula, $floatFormatCB);
    $$dest_sheet_ref->write_formula($row, col2int($cols{actual_yds_per_sec}), $actSpeedFormula, $floatFormatCB);
    my $lookupAvSpeedFormula;
    if($g_jumps == 0)
    {
      #Flat race
      $lookupAvSpeedFormula = "=VLOOKUP($cols{prev_going_ordinal}$form_idx,I2:J8,2,FALSE)";
    }
    else
    {
      $lookupAvSpeedFormula = "=(INDEX(\$I\$2:\$K\$8,MATCH($cols{prev_going_ordinal}$form_idx,\$I\$2:\$I\$8,0),2)*($cols{actual_prev_distance}$form_idx/1000)) + INDEX(\$I\$2:\$K\$8,MATCH($cols{prev_going_ordinal}$form_idx,\$I\$2:\$I\$8,0),3)";
    }

    $$dest_sheet_ref->write_formula($row, col2int($cols{avg_yds_per_sec}), $lookupAvSpeedFormula, $floatFormatCB);   
    $$dest_sheet_ref->write_formula($row, col2int($cols{diff_yds_per_sec}), $speedDiffFormula, $floatFormatCB);
    $$dest_sheet_ref->write_formula($row, col2int($cols{pred_yds_per_sec}), $predSpeedFormula, $floatFormatCB);
    $$dest_sheet_ref->write_formula($row, col2int($cols{pred_time}), $predTimeFormula, $floatFormatCB);
    $$dest_sheet_ref->write_formula($row, col2int($cols{pred_adj_time}), $sec2TimeFormula, $floatFormatCB);
    $row++;
  }
}

sub convertGoing {
  print "convertGoing\n" if $debug;
  my $going = shift;
  my %goingTable;	
  if($g_jumps == 0)
  {
    #Hard
    $goingTable{'1'}   = ( [qr{^h.*d.*}i]);
    #Firm
    $goingTable{'2'}   = [ qr{^f.*m.*}i ];
    #Good/Firm
    $goingTable{'3'} = [ qr{^g.*d{1}/f.*m,?}i, qr{^g.*d *to *f.*m *.*$}i];
    #Good
    $goingTable{'4'}   = [ qr{^g.*d$}i, qr{^g.*d-g.*$}i, qr{^s.*d$}i, qr{^g.*d,.*$}i];
    #Good/Soft
    $goingTable{'5'} = [ qr{^g.*d/s.*t.*}i, qr{^Standard / Slow}i, qr{^g.*d to s.*t.*}i, qr{^Standard to slow}i, qr{^yielding\s*$}i, qr{^yielding\s*\(}i];
    #Soft
    $goingTable{'6'}   = [ qr{^s.*t,?$}i, qr{^Soft,.*$}i, qr{^Soft-.*}i, qr{^Soft .+}i ];
    #Heavy
    $goingTable{'7'}  = [ qr{^h.*y.*}i ];
  }
  else
  {
    #Firm
    $goingTable{'1'}   = [ qr{^f.*m.*}i ];
    #Good/Firm
    $goingTable{'2'} = [ qr{^g.*d{1}/f.*m,?}i, qr{^g.*d *to *f.*m *.*$}i];
    #Good
    $goingTable{'3'}   = [ qr{^g.*d$}i, qr{^g.*d-g.*$}i, qr{^s.*d$}i, qr{^good,}i, qr{^g.*d,.*$}i, qr{^g.*d \(.*}i];
    #Good/Soft
    $goingTable{'4'} = [ qr{^g.*d/s.*t.*}i, qr{^s.*d/s.*w.*}i, qr{^g\S+d to s.*t.*}i, qr{^yielding\s*$}i, qr{^yielding\s*\(}i, qr{^Standard / Slow}i, qr{^Standard\s*to\s*slow}i];
    #Soft
    $goingTable{'5'}   = [ qr{^s.*t$}i, qr{^s.*t,.*$}i, qr{^s.*t-.*$}i, qr{^s.*t \(.*$}i, qr{^yielding\s*$}i, qr{^yielding to soft}i];
    #Soft/Heavy
    $goingTable{'6'}   = [ qr{^s.*t/h.*y$}i, qr{^soft to heavy}i ];
    #Heavy
    $goingTable{'7'}  = [ qr{^h.*y.*}i ];
  }
  
  
 print $debugFH "$going\n" if $debug;
  for my $goingKey (keys %goingTable)
  {
    print $debugFH  "$goingKey :\n" if $debug;
    for my $i ( 0..$#{$goingTable{$goingKey}} )
    {
      print $debugFH "\t$goingTable{$goingKey}[$i] $going\n" if $debug;
      if($going && $going =~/$goingTable{$goingKey}[$i]/i) 
      {
        print  $debugFH "\t$goingKey $goingTable{$goingKey}[$i] $going\n" if $debug;
        $a = 1 * $goingKey;
        return $a;
      }
    }
  }

  return "-1";

}

sub convertBeatenLengths {
  my $dist = shift;
  my $convDist;
  
  if($dist =~ /(\d*).*frac(\d)(\d)/ )
  {
    #Has a beaten distance in lengths + a fraction
    $convDist = $1 + ($2/$3);
  }
  elsif($dist =~ /^\d/)
  {
    #Has a beaten distance in std lengths
    $convDist = $dist;
  }
  elsif($dist =~ /^nk$/)
  {
    #Beaten by a neck
    $convDist = 0.3;
  }
  elsif($dist =~ /^hd$/)
  {
    #Beaten by a head
    $convDist = 0.2;
  }
  elsif($dist =~ /^s.h$/)
  {
    #Beaten by a neck
    $convDist = 0.1;
  }
  
  return $convDist;
}

sub convertDistance {
  my $dist = shift;
  my $convDist;
  
  if($dist)
  {
    if($dist =~ /(\d+)m *(\d+)f *(\d+)y/)
    {
      $convDist = (1760 * $1) + (220 * $2) + $3;
    }
    elsif($dist =~ /(\d+)m *(\d)f/)
    {
      $convDist = (1760 * $1) + (220 * $2);
    }
    elsif($dist =~ /(\d+)m *(\d+)y/)
    {
      $convDist = (1760 * $1) + $2;
    }	
    elsif($dist =~ /(\d+)m/)
    {
      $convDist = 1760 * $1;
    }
    elsif($dist =~ /(\d+)f *(\d+)y/)
    {	
      $convDist = 220 * $1 + $2;
    }
    elsif($dist =~ /(\d+)f/)
    {	
      $convDist = 220 * $1;
    }
    else
    {
      $convDist = -1;
    }
  }

  return $convDist;
}

sub convertTime {
  my $time = shift;
  my $convTime;
  
  if($time =~ /(\d+)m *(\d+\.\d*)s/)
  {
    $convTime = (60 * $1) + $2;
  }
  elsif ($time =~ /(\d+\.\d*)s/)
  {
    $convTime = $1;
  }
  else
  {
    $convTime = -1;
  }
  return $convTime;
}

sub getAvSpeed {
#This is required as the repeated formula $lookupAvSpeedFormula is returning
#an error in excel about mixed data types.  Instead of doing the lookup in excel
#use this sub to get the correct average speed for that particular going.
  my $going = shift;
  my %speedLookup;
  $speedLookup{'1'} = [ 1, 19.00 ];
  $speedLookup{'2'} = [ 2, 18.61 ];
  $speedLookup{'3'} = [ 3, 18.15 ];
  $speedLookup{'4'} = [ 4, 18.06 ];
  $speedLookup{'5'} = [ 5, 17.11 ];
  $speedLookup{'6'} = [ 6, 16.86 ];
  $speedLookup{'7'} = [ 7, 16.00 ];
  
  return $speedLookup{$going}[1];
}
  
sub writeLookupTable {
  print "writeLookupTable\n" if $debug;
  my $dest_sheet_ref = shift;
  my $row = 0;
  my $col = 7;
  if($g_jumps == 0)
  {
    $$dest_sheet_ref->write($row,  $col,    "Going",     $strFormatLimeCB);
    $$dest_sheet_ref->write($row, ($col+1), "Value",     $strFormatLimeCB);
    $$dest_sheet_ref->write($row, ($col+2), "AvSpeed",   $strFormatLimeCB);
    $$dest_sheet_ref->write($row, ($col+3), "Dist(f)",   $strFormatLimeCB);
    $$dest_sheet_ref->write($row, ($col+4), "Dist(yds)", $strFormatLimeCB);
    
    my @goingAoA = (['hard',  1, 19.00, 5,  1100],
            ['firm',  2, 18.61, 6,  1320],
            ['gdfirm',3, 18.15, 7,  1540],
            ['good',  4, 18.06, 8,  1760],
            ['gdsoft',5, 17.11, 9,  1980],
            ['soft',  6, 16.86, 10, 2200],
            ['heavy', 7, 16.00, 11, 2420]);		
                        
    $row = 1;
    for my $x (0..$#goingAoA)
    {
      my $colA = $col;
      for my $y (0..$#{$goingAoA[$x]})
      {
        if($y == 0)
        {
          $$dest_sheet_ref->write($row, $colA, $goingAoA[$x][$y], $strFormatLimeCB);
        }
        elsif($y == 2)
        {
          $$dest_sheet_ref->write_number($row, $colA, $goingAoA[$x][$y], $floatFormatLimeCB);
        }
        else
        {

          $$dest_sheet_ref->write_number($row, $colA, $goingAoA[$x][$y], $intFormatLimeCB);
        }
        
        $colA++;
      }
      $row++;
    }
  }
  else # Jumps race
  {
    $$dest_sheet_ref->write($row,  $col,    "Going",     $strFormatLimeCB);
    $$dest_sheet_ref->write($row, ($col+1), "Value",     $strFormatLimeCB);
    $$dest_sheet_ref->write($row, ($col+2), "m",   $strFormatLimeCB);
    $$dest_sheet_ref->write($row, ($col+3), "x",   $strFormatLimeCB);
    
    my @goingAoA = (
        ['firm',   1, -0.2426, 16.454],
        ['gdfirm', 2, -0.2079, 15.946],
        ['good',   3, -0.3019, 16.109],
        ['gdsoft', 4, -0.3513, 15.934],
        ['soft',   5, -0.3126, 15.326],
        ['sft/hvy',6, -0.4607, 15.805],
        ['heavy',  7, -0.2379, 14.431]);
        
    $row = 1;
    for my $x (0..$#goingAoA)
    {
      my $colA = $col;
      for my $y (0..$#{$goingAoA[$x]})
      {
        if($y == 0)
        {
          $$dest_sheet_ref->write($row, $colA, $goingAoA[$x][$y], $strFormatLimeCB);
        }
        elsif($y == 1)
        {
          $$dest_sheet_ref->write_number($row, $colA, $goingAoA[$x][$y], $intFormatLimeCB);
        }
        elsif($y == 2)
        {
          $$dest_sheet_ref->write_number($row, $colA, $goingAoA[$x][$y], $float4FormatLimeCB);
        } 
        elsif($y == 3)
        {
          $$dest_sheet_ref->write_number($row, $colA, $goingAoA[$x][$y], $floatFormatLimeCB);
        }        
        $colA++;
      }
      $row++;
    }

  }
}



sub writeRaceInfo {
  print "writeRaceInfo\n" if $debug;
  my $dest_sheet_ref = shift;
  my @raceInfos = qw/Link Track Runners Date Going Dist Class/;
  my @raceInfo_headers = qw/Link Meeting Runners Date Info Going Distance/;
  my $row = 0;
  my $col = 1;

  my $course = $g_activeRace->{$COURSE};
  my $url = $g_activeRace->{$URL};
  my $raceTxt = $g_activeRace->{$TIME} . "_" . $g_activeRace->{$COURSE};
  $$dest_sheet_ref->write_url(($row  ), $col, $url, $raceTxt, $entryFormatUrl);
  my $runners = $g_activeRace->{$NUM_RUNNERS} . " runners";
  $$dest_sheet_ref->write(($row+1), $col, $runners);
  $$dest_sheet_ref->write(($row+2), $col, $g_activeRace->{$DATE});
  $$dest_sheet_ref->write(($row+3), $col, $g_activeRace->{$TITLE});
  
  my $dist = $g_activeRace->{$DISTANCE};
  my $convDist = convertDistance($dist);
  my $format = getFormat($convDist, "int",  "yes");			
  $$dest_sheet_ref->write(($row+4), $col, $dist, $strFormatLimeCB);
  $$dest_sheet_ref->write_number(($row+4), ($col+1), $convDist, $format );
  
  $$dest_sheet_ref->write_comment($row, $col, $g_activeRace->{$INFO}, width => 400 , height => 300);
  
  my $going = $g_activeRace->{$GOING};
  my $convGoing = convertGoing($going);
  $format = getFormat($convGoing, "int", "yes");
  my $a = 1 * $convGoing;
  $$dest_sheet_ref->write(($row+5), $col, $going, $strFormatLimeCB);
  $$dest_sheet_ref->write(($row+5), ($col+1), $a, $format);
  
  my $todayAvgSpeedFormula = '=VLOOKUP(C6,I2:J8,2,FALSE)';
  my $predAvgSpeedFormula = '=(INDEX($I$2:$K$8,MATCH(C6,$I$2:$I$8,0),2)*(C5/1000)) + INDEX($I$2:$K$8,MATCH(C6,$I$2:$I$8,0),3)';
  if($g_jumps == 0)
  {
    $$dest_sheet_ref->write_formula(($row+5), ($col+2), $todayAvgSpeedFormula, $format);
  }
  else
  {
    $$dest_sheet_ref->write_formula(($row+5), ($col+2), $predAvgSpeedFormula, $floatFormatCB);
  } 
  
  my $category = $g_activeRace->{$CATEGORY};
  $$dest_sheet_ref->write(($row+6), $col, $category, $entryFormat1) if ref $g_activeRace->{$CATEGORY} ne 'HASH';
 
  my $winPrize = $g_activeRace->{$WIN_PRIZE};
  $$dest_sheet_ref->write(($row+7), ($col), $winPrize , $entryFormat1);
}

sub getFormat {
  my $val = shift;
  my $type = shift;
  my $critical = shift;
  my $format;

  if(!$val)
  {
    return 0;
  }
  
  if ($type eq "str")
  {
    $format = $strFormatHLCB;
    if($val eq "NOMATCH")
    {
      $format = $strFormatRedCB
    }
    elsif($critical eq "yes")
    {
      $format = $strFormatSilverCB
    }
  }
  elsif ($type eq "int")
  {
    $format = $intFormatHLCB;
    if($val == -1)
    {
      $format = $intFormatRedCB
    }
    elsif($critical eq "yes")
    {
      $format = $intFormatSilverCB
    }
  }
  elsif ($type eq "float")
  {
    $format = $floatFormatHLCB; 
    if($val eq "N/A")
    {
        $format = $floatFormatSilverCB
    }
    elsif($val == -1)
    {
      $format = $floatFormatRedCB
    }
    elsif($critical eq "yes")
    {
      $format = $floatFormatSilverCB
    } 
  }
  return $format;
}	


sub formatDateTime {
  my $DT_enum = shift;
  my $datetime = shift;
  my $res;
  if($$datetime =~ /(\d{4})-(\d{2})-(\d{2})T(\d{2}:\d{2})/)
  { 
    if($DT_enum eq $DATE)
    {
      $res = $3 . "-" . $2 . "-" . $1;
    }
    elsif($DT_enum eq $TIME)
    {
      $res = $4;
    }
  }
  return $res;
}

sub writeHorseInfo {
  print "writeHorseInfo\n" if $debug;
  my $dest_sheet_ref = shift;
  my $row = $infoStartRow;
  my $info = "";
  my $raceSize = $#{$g_activeRace->{$ENTRANTS}{$ENTRANT}};
 
  for my $iHorses ( 0..$raceSize ) 
  {
    my $horse = $g_activeRace->{$ENTRANTS}{$ENTRANT}[$iHorses];  
    my $prevPos = "";
    my $prevTrack = 0;
    my $prevRan = 0;
    my $prevPrize = "";
    if(ref($horse->{$LAST_RACE}) eq "HASH")
    {
        $prevPos = $horse->{$LAST_RACE}{$POSITION};
        $prevTrack = $horse->{$LAST_RACE}{$COURSE};
        $prevRan = $horse->{$LAST_RACE}{$NUM_RUNNERS};
    }
    
    my $prevFullPos = $prevPos; # Take copy as this is now a column entry: 2/11 becomes 2 in the substitution (next line)
    $prevPos =~ s/(\d+)\/\d+/$1/ if $prevPos;
    my $prevExists = 0;
    $prevExists = 1 if $prevTrack || $prevRan || $prevPos;   
    my $lastAnalysis = "";
    my $originalRating = "Rating: ";
    my $formWatch = "";
    my $lastRan = "";
    my $col;
           
    for my $horseKey (keys %{ $horse })
    {

      my $localFormat = $strFormatGreenCB;
      if($HORSE_NAME eq $horseKey)
      {    
        $col = $cols{horse};
        $$dest_sheet_ref->write_url(($row + 1), col2int($col), $horse->{$HORSE_URL}, $horse->{$HORSE_NAME}, $entryFormatUrl );
      }
      elsif($TRAINER_NAME eq $horseKey)
      {
        $$dest_sheet_ref->write_url(($row + 1), col2int($cols{trainer}), $horse->{$TRAINER_URL}, $horse->{$TRAINER_NAME}, $entryFormatUrl );
      }
      elsif($WEIGHT eq $horseKey)
      {
        my $weight = $horse->{$WEIGHT};
        $weight =~ s/(\d+-\d+).*/$1/;
        $$dest_sheet_ref->write(($row + 1), col2int($cols{todays_weight}), $weight, $entryFormat1);
      }
      elsif($FORM eq $horseKey)
      {
        if(ref($horse->{$FORM}) ne "HASH") 
        {
            $$dest_sheet_ref->write(($row + 1), col2int($cols{form}), $horse->{$FORM}, $strFormatCB );
        }
     
        # my $winPrize = $horse->{$key}{'PrizeMoney'}{'decimal'}[0];
        # $$dest_sheet_ref->write(($row + 1), ($col + 5), $winPrize, $entryFormatRight) if $winPrize;     
      }
      elsif($FORM_WATCH eq $horseKey)
      {
        if(ref($horse->{$FORM_WATCH}) ne "HASH")
        {
          $formWatch = $horse->{$FORM_WATCH};
        }
      }
      elsif($LAST_RAN eq $horseKey)
      {
        if(ref($horse->{$LAST_RAN}) ne "HASH")
        {
          $$dest_sheet_ref->write(($row + 1), col2int($cols{since_last}), $horse->{$LAST_RAN}, $strFormatCB);
        }
      }
      elsif($LAST_RACE eq $horseKey)
      {
        for my $key (keys %{ $horse->{$LAST_RACE} })
        {
            if($WEIGHT eq $key)
            {
                my $prevWgt = $horse->{$LAST_RACE}{$WEIGHT};
                $prevWgt =~ s/(\d+-\d+).*/$1/;
                $$dest_sheet_ref->write(($row + 1), col2int($cols{prev_weight}), $prevWgt, $entryFormat1);   
            }
            elsif($POSITION eq $key)
            {
                my $format = getFormat($prevFullPos, "str", "yes");
                if($prevFullPos =~ /LRx/)
                {
                    $format = $strFormatCB;
                }
                $$dest_sheet_ref->write(($row + 1), col2int($cols{prev_pos}), $prevFullPos, $format) if $prevFullPos;
            } 
            elsif($PRIZE eq $key)
            {
                my $prize =  $horse->{$LAST_RACE}{$PRIZE};
                if(ref($horse->{$LAST_RACE}{$PRIZE}) ne "HASH")
                {
                    $$dest_sheet_ref->write(($row + 1), col2int($cols{prev_prize}), $prize, $entryFormatRight) if $prize;                
                }
                
            } 
            elsif($GOING eq $key)
            {
                my $going = $horse->{$LAST_RACE}{$GOING};
                if($going)
                {
                  my $convGoing = convertGoing($going);
                  my $format = getFormat($convGoing, "int", "yes");       
                  $$dest_sheet_ref->write(($row + 1), col2int($cols{prev_going_str}), $going, $entryFormatLeft);
                  $$dest_sheet_ref->write_number(($row + 1), col2int($cols{prev_going_ordinal}), $convGoing, $format);
                }
                elsif(!$going && $prevExists)
                {       
                    $going = -1;
                    my $format = getFormat($going, "int", "no"); 
                    $$dest_sheet_ref->write_number(($row + 1), col2int($cols{prev_going_ordinal}), $going, $format);
                }	
            }
            elsif($BEATEN_LENGTHS eq $key)
            {
                my $format = $entryFormat1;
                my $prevBeatDist = $horse->{$LAST_RACE}{$BEATEN_LENGTHS};
  
                if($prevFullPos && $prevFullPos =~ /^[A-Z]+$/)
                {
                    ## race incident (fell (F), unseated rider (UR) etc) 
                    $prevBeatDist =  "N/A";    
                    $format = getFormat(1, "str", "yes");
                }
                elsif($prevFullPos =~ /LRx/)
                {
                    ## Last Race data doesn't match days since last ran
                    $prevBeatDist = "LRx";
                    $format = $strFormatCB;
                }
                elsif($prevPos ne "No Form" && $prevPos ne "-" && $prevPos > 1 && $prevBeatDist == -1)
                {
                    ## race position but no beaten distance
                    $prevBeatDist = -1;
                    $format = getFormat($prevBeatDist, "float", "no");
                }
                elsif($prevBeatDist && ref $prevBeatDist ne "HASH" && $prevExists)
                {
                    ## valid previous race data available
                    $prevBeatDist = convertBeatenLengths($prevBeatDist);      
                    $format = getFormat(1, "float", "yes");
                }
                elsif($prevBeatDist && ref $prevBeatDist ne "HASH" && !$prevExists)
                {
                    ## unlikely condition, valid distance beaten but no other race data...
                    $prevBeatDist = convertBeatenLengths($prevBeatDist);
                    $format = getFormat(1, "float", "yes");
                }
                elsif( (!$prevBeatDist || ref $prevBeatDist eq "HASH")  && $prevExists)
                {
                    ## possible condition, no distance but other race data exists
                    if($prevPos && $prevPos == 1)
                    {
                        ## 1st place
                        $prevBeatDist = 0; 
                        $format = getFormat(1, "float", "yes");
                    }
                    elsif($prevPos && $prevPos =~ /^[A-Z]+$/)
                    {
                        ## race incident (fell (F), unseated rider (UR) etc) 
                        $prevBeatDist =  $prevPos;    
                        $format = getFormat(1, "float", "yes");
                    }
                    elsif((!$prevBeatDist || ref $prevBeatDist eq "HASH") && !$prevExists)
                    {                 
                      ## No race history
                      #$format = getFormat(1, "float", "yes"); # send '1" to get a valid format
                    }  
                    else
                    {
                        ## no distance or position but race data exists, format for error
                        $prevBeatDist = -1;
                        $format = getFormat($prevBeatDist, "float", "yes");
                    }
                }
                $$dest_sheet_ref->write(($row + 1), col2int($cols{prev_beaten_lengths}), $prevBeatDist, $format);
            }      
            elsif($ANALYSIS eq $key)
            {
                if(ref($horse->{$LAST_RACE}{$ANALYSIS}) ne "HASH")
                {
                  $lastAnalysis .= $horse->{$LAST_RACE}{$ANALYSIS};
                }
            }
            elsif($CLASS eq $key)
            {
                my $prevClass = $horse->{$LAST_RACE}{$CLASS};
                $$dest_sheet_ref->write(($row + 1), col2int($cols{prev_class}), $prevClass, $entryFormatLeft) if ref $prevClass ne "HASH"; 
            }
            elsif($DISTANCE eq $key)
            {
                my $dist = $horse->{$LAST_RACE}{$DISTANCE};
                my $convDist = convertDistance($dist);
                my $format = getFormat($convDist, "int", "no");
                $$dest_sheet_ref->write(($row + 1), col2int($cols{prev_distance_str}), $dist, $entryFormat1);
                $$dest_sheet_ref->write(($row + 1), col2int($cols{prev_distance_yds}), $convDist, $format);
            }
            elsif($WINNING_TIME eq $key)
            {
                my $convTime;
                my $winTime = $horse->{$LAST_RACE}{$WINNING_TIME};
                if($winTime)
                {
                    $convTime = convertTime($winTime);
                    my $format = getFormat($convTime, "float", "no");
                    $$dest_sheet_ref->write(($row + 1), col2int($cols{winning_time_str}), $winTime, $entryFormat1);
                    $$dest_sheet_ref->write(($row + 1), col2int($cols{winning_time_sec}), $convTime, $format);
                }
                else
                {
                    $convTime = -1;
                    my $format = getFormat($convTime, "float", "yes");
                    $$dest_sheet_ref->write(($row + 1), col2int($cols{winning_time_sec}), $convTime, $format);
                }
            }               
            
        }    
      }      
      elsif($JOCKEY_NAME eq $horseKey)
      {
        if(ref($horse->{$JOCKEY_NAME}) ne "HASH")
        {    
          my $jockeyClaim = $horse->{$JOCKEY_CLAIM};
          my $format = $entryFormatUrl;
          
          if(ref($jockeyClaim) ne "HASH")
          {
            ## write_url format was getting screwed up when combined with my formats, therefore had to call write_url in this branching if..elsif where format is only applied if there is a jockey penalty
            if($jockeyClaim eq 3)
            {
              $format = $entryFormatUrlOrangeBg;
            }
            elsif($jockeyClaim eq 7)
            {
              $format = $entryFormatUrlRedBg;
            }
          }           

          $$dest_sheet_ref->write_url(($row + 1), col2int($cols{jockey}), $horse->{$JOCKEY_URL}, $horse->{$JOCKEY_NAME}, $format);
        }
      }
      elsif($RATING eq $horseKey)
      {
        if(ref($horse->{$RATING}) ne "HASH")
        {
          $originalRating .= $horse->{$RATING};
        }
      }

    }
    $row++;	
    
    # Write Comments
    my $comment = $originalRating . "\n" . $formWatch . "\n" . $prevPrize;
    $$dest_sheet_ref->write_comment($row, col2int($cols{horse}), $comment, width => 350 , height => 200);
    $$dest_sheet_ref->write_comment($row, col2int($cols{prev_pos}), $lastAnalysis, width => 200 , height => 100);
  }
  addDynamicFormulas(\$$dest_sheet_ref, $raceSize, $g_jumps);
}

sub getDistanceBeaten {
  my $prevBeatRes = shift;
  my $pos = shift;
  if($pos == 1){return "0.00"};
  my $total = 0;
  foreach my $i (0..($pos-1))
  {
    unless(defined(${$prevBeatRes}[$i])){return -1}
    $total += ${$prevBeatRes}[$i];
  }
  return $total;
}

sub addFormats {	
  my $dest_book_ref = shift;

  #### $headFormat1 ####
  $headFormat1 = $$dest_book_ref->add_format();
  $headFormat1->set_border(5);
  $headFormat1->set_text_wrap(1);
  
  #### $headFormat2 ####
  $headFormat2 = $$dest_book_ref->add_format();
  $headFormat2->copy($headFormat1);
  $headFormat2->set_align('center');

  #### $strFormatCB ####
  $strFormatCB = $$dest_book_ref->add_format();
  $strFormatCB->copy($headFormat2);
  $strFormatCB->set_align('center');	
  #### $floatFormatCB ####
  $floatFormatCB = $$dest_book_ref->add_format();
  $floatFormatCB->copy($headFormat2);
  $floatFormatCB->set_num_format('0.00');
  #### $intFormatCB ####
  $intFormatCB = $$dest_book_ref->add_format();
  $intFormatCB->copy($headFormat2);
  $intFormatCB->set_num_format('0');	
   
  #### $strFormatGreenCB ####
  $strFormatGreenCB = $$dest_book_ref->add_format();
  $strFormatGreenCB->copy($strFormatCB);
  $strFormatGreenCB->set_bg_color('green');	
  #### $floatFormatGreenCB ####
  $floatFormatGreenCB = $$dest_book_ref->add_format();
  $floatFormatGreenCB->copy($floatFormatCB);
  $floatFormatGreenCB->set_num_format('0.00');
  $floatFormatGreenCB->set_bg_color('green');
  #### $intFormatGreenCB ####
  $intFormatGreenCB = $$dest_book_ref->add_format();
  $intFormatGreenCB->copy($intFormatCB);
  $intFormatGreenCB->set_num_format('0');
  $intFormatGreenCB->set_bg_color('green');
  
  #### $strFormatHLCB #### Highlight Center Bold
  $strFormatHLCB = $$dest_book_ref->add_format();
  $strFormatHLCB->copy($strFormatCB);
  $strFormatHLCB->set_bg_color('cyan');	
  #### $floatFormatHLCB ####
  $floatFormatHLCB = $$dest_book_ref->add_format();
  $floatFormatHLCB->copy($floatFormatCB);
  $floatFormatHLCB->set_num_format('0.00');
  $floatFormatHLCB->set_bg_color('cyan');
  #### $intFormatHLCB ####
  $intFormatHLCB = $$dest_book_ref->add_format();
  $intFormatHLCB->copy($intFormatCB);
  $intFormatHLCB->set_num_format('0');
  $intFormatHLCB->set_bg_color('cyan');
  
  #### $strFormatLimeCB ####
  $strFormatLimeCB = $$dest_book_ref->add_format();
  $strFormatLimeCB->copy($strFormatCB);
  $strFormatLimeCB->set_bg_color('lime');	
  #### $floatFormatLimeCB ####
  $floatFormatLimeCB = $$dest_book_ref->add_format();
  $floatFormatLimeCB->copy($floatFormatCB);
  $floatFormatLimeCB->set_num_format('0.00');
  $floatFormatLimeCB->set_bg_color('lime');
  #### $float4FormatLimeCB ####
  $float4FormatLimeCB = $$dest_book_ref->add_format();
  $float4FormatLimeCB->copy($floatFormatCB);
  $float4FormatLimeCB->set_num_format('0.0000');
  $float4FormatLimeCB->set_bg_color('lime');
  #### $intFormatLimeCB ####
  $intFormatLimeCB = $$dest_book_ref->add_format();
  $intFormatLimeCB->copy($intFormatCB);
  $intFormatLimeCB->set_num_format('0');
  $intFormatLimeCB->set_bg_color('lime');	
  
  #### $strFormatRedCB ####
  $strFormatRedCB = $$dest_book_ref->add_format();
  $strFormatRedCB->copy($strFormatCB);
  $strFormatRedCB->set_bg_color('red');	
  #### $floatFormatRedCB ####
  $floatFormatRedCB = $$dest_book_ref->add_format();
  $floatFormatRedCB->copy($floatFormatCB);
  $floatFormatRedCB->set_num_format('0.00');
  $floatFormatRedCB->set_bg_color('red');
  #### $intFormatRedCB ####
  $intFormatRedCB = $$dest_book_ref->add_format();
  $intFormatRedCB->copy($intFormatCB);
  $intFormatRedCB->set_num_format('0');
  $intFormatRedCB->set_bg_color('red');	
  
  #### $strFormatOrangeCB ####
  $strFormatOrangeCB = $$dest_book_ref->add_format();
  $strFormatOrangeCB->copy($strFormatCB);
  $strFormatOrangeCB->set_bg_color('orange');	
  
  #### $strFormatSilverCB ####
  $strFormatSilverCB = $$dest_book_ref->add_format();
  $strFormatSilverCB->copy($strFormatCB);
  $strFormatSilverCB->set_bg_color('silver');	
  #### $floatFormatSilverCB ####
  $floatFormatSilverCB = $$dest_book_ref->add_format();
  $floatFormatSilverCB->copy($floatFormatCB);
  $floatFormatSilverCB->set_num_format('0.00');
  $floatFormatSilverCB->set_bg_color('silver');
  #### $intFormatSilverCB ####
  $intFormatSilverCB = $$dest_book_ref->add_format();
  $intFormatSilverCB->copy($intFormatCB);
  $intFormatSilverCB->set_num_format('0');
  $intFormatSilverCB->set_bg_color('silver');	
    
  #### $entryFormat1 ####		
  $entryFormat1 = $$dest_book_ref->add_format();
  $entryFormat1->set_align('center');	
  
  #### $entryFormatRight ####		
  $entryFormatRight = $$dest_book_ref->add_format();
  $entryFormatRight->set_align('right');	
  
  #### $entryFormatLeft ####	
  $entryFormatLeft = $$dest_book_ref->add_format();
  $entryFormatLeft->set_align('left');
  
  ####  $entryFormatUrlRedBg ####	
  $entryFormatUrlRedBg = $$dest_book_ref->add_format();
  $entryFormatUrlRedBg->set_underline();
  $entryFormatUrlRedBg->set_color('blue');
  $entryFormatUrlRedBg->set_bg_color('red');
  
  ####  $entryFormatUrlOrangeBg ####	
  $entryFormatUrlOrangeBg = $$dest_book_ref->add_format();
  $entryFormatUrlOrangeBg->set_underline();
  $entryFormatUrlOrangeBg->set_color('blue');
  $entryFormatUrlOrangeBg->set_bg_color('52');
  
  ####  $entryFormatUrl ####	
  $entryFormatUrl = $$dest_book_ref->add_format();
  $entryFormatUrl->set_underline();
  $entryFormatUrl->set_color('blue');

}

sub setCellDimension {
  print "setCellDimension\n" if $debug;
  my $dest_sheet_ref = shift;
  $$dest_sheet_ref->set_column(0, 0, 22);
  $$dest_sheet_ref->set_column(1, 4, 8);
  $$dest_sheet_ref->set_column(5, 5, 8);
  $$dest_sheet_ref->set_column(6, 6, 15);
  $$dest_sheet_ref->set_column(7, 7, 14);
  $$dest_sheet_ref->set_column(8, 8, 8);
  $$dest_sheet_ref->set_column(9, 9, 8);
  $$dest_sheet_ref->set_column(10, 10, 15);
  $$dest_sheet_ref->set_column(15, 15, 10);
  $$dest_sheet_ref->set_column(18, 18, 24);
  $$dest_sheet_ref->set_row($infoStartRow, 25);
}

sub writeRaceHeading {
  print "writeRaceHeading\n" if $debug;
  my $dest_sheet_ref = shift;
  my $row = 0;
  my $col = 0;
  $$dest_sheet_ref->write(($row  ), $col, "Race", $headFormat1);
  $$dest_sheet_ref->write(($row+1), $col, "Runners", $headFormat1);
  $$dest_sheet_ref->write(($row+2), $col, "Date", $headFormat1);
  $$dest_sheet_ref->write(($row+3), $col, "Info", $headFormat1);
  $$dest_sheet_ref->write(($row+4), $col, "Distance (f)", $headFormat1);
  $$dest_sheet_ref->write(($row+5), $col, "Going + PredAvSpeed", $headFormat1);
  $$dest_sheet_ref->write(($row+6), $col, "Class", $headFormat1);
  $$dest_sheet_ref->write(($row+7), $col, "Prize", $headFormat1);
}


sub writeHorseHeadings {
  print "writeHorseHeadings\n" if $debug;
  my $dest_sheet_ref = shift;
  my $row = $infoStartRow;
  my $col = 0;
  $$dest_sheet_ref->write($row, col2int($cols{horse}),  "Horse Name", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{pred_time}),  "New Time (s)", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{pred_adj_time}),  "2sec Time (s)", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{prev_pos}),  "Prev Pos", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{form}),  "Form", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{since_last}),  "Since Last", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{trainer}),  "Trainer", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{jockey}),  "Jockey", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{todays_weight}),  "Wgt", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{prev_weight}),  "Prev Wgt", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{prev_prize}),  "Prev Prize", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{prev_class}), "Prev Class", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{prev_distance_str}), "Prev Dist", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{prev_distance_yds}), "Prev Dist(yds)", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{prev_beaten_lengths}), "Prev BeatLen", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{actual_prev_distance}), "Actual Dist(yds)", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{winning_time_str}), "Winning Time", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{winning_time_sec}), "Winning Time(s)", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{prev_going_str}), "Prev Going", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{prev_going_ordinal}), "Prev GoingNo", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{avg_yds_per_sec}), "Average yds/s", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{actual_yds_per_sec}), "Actual y/s", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{diff_yds_per_sec}), "Actual-Av y/s (%)", $headFormat2);
  $$dest_sheet_ref->write($row, col2int($cols{pred_yds_per_sec}), "Pred Speed yds/s", $headFormat2);
}

sub writeVersionInfo {
  print "writeVersionInfo\n" if $debug;
  my $dest_sheet_ref = shift;
  my $version = shift;
  my $info = "xmlHorseBuilder " . $version;
  $$dest_sheet_ref->write(0, 14, $info, $headFormat1);
}
