using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RacingWebScraper
{
    public partial class SLifeRacingScraper
    {
        String ScrapeTextContent(AngleSharp.Dom.IDocument document, String selector)
        {
            var selected = document.QuerySelector(selector);
            if (selected != null)
            {
                return selected.TextContent;
            }
            else
            {
                return "";
            }
        }

        String ScrapeTextContent(AngleSharp.Dom.IElement element, String selector)
        {
            var selected = element.QuerySelector(selector);
            if (selected != null)
            {
                return selected.TextContent;
            }
            else
            {
                return "";
            }
        }

        String ScrapeUrl(AngleSharp.Dom.IElement element, String selector)
        {
            IElement selElem = element.QuerySelector(selector);
            if (selElem != null)
            {
                return selElem.GetAttribute("href");
            }
            return "UNKNOWN";
                
        }

        String ScrapeUrl(AngleSharp.Dom.IDocument document, String selector)
        {
            return document.QuerySelector(selector).GetAttribute("href");
        }
        private int ScrapeIntFromTextContent(AngleSharp.Dom.IDocument document, String selector, String rxCapture)
        {
            var textContent = ScrapeTextContent(document, selector);

            if (textContent != null)
            {
                Regex rx = new Regex(rxCapture);
                Match match = rx.Match(textContent);
                if (match.Success)
                {
                    int scraped;
                    bool res = int.TryParse(match.Groups[1].Value, out scraped);
                    if (res == true)
                    {
                        return scraped;
                    }
                }
            }

            return -1;
        }
        private int ScrapeIntFromTextContent(AngleSharp.Dom.IElement element, String selector, String rxCapture)
        {
            var textContent = ScrapeTextContent(element, selector);

            if (textContent != null)
            {
                Regex rx = new Regex(rxCapture);
                Match match = rx.Match(textContent);
                if (match.Success)
                {
                    int scraped;
                    bool res = int.TryParse(match.Groups[1].Value, out scraped);
                    if (res == true)
                    {
                        return scraped;
                    }
                }
            }

            return -1;
        }
        // #content > div > div > div > div.main-layout-left-column > div.main-layout-block.main-layout-order-desktop-1.main-layout-order-handheld-1 > div > div.horseProfile-results > div > table > tbody > tr:nth-child(1) > td:nth-child(5)
        private String ScrapeStringFromTextContent(IDocument document, String selector, String rxCapture)
        {
            var textContent = ScrapeTextContent(document, selector);
            if (textContent != null)
            {
                Regex rx = new Regex(rxCapture);
                Match match = rx.Match(textContent);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                
            }

            return "";
        }

        private String ScrapeStringFromTextContent(AngleSharp.Dom.IElement element, String selector, String rxCapture)
        {
            var textContent = ScrapeTextContent(element, selector);
            if (textContent != null)
            {
                Regex rx = new Regex(rxCapture);
                Match match = rx.Match(textContent);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return "";
        }

    }

}
