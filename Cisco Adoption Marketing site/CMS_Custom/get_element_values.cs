using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ingeniux.CMS
{
  /*
   *************************************************
   *************************************************
   *************************************************
   *  Note: this document is mirrored in custom/get_element_values.cs and is here for tracking purposes *
   *  Hand deliver file to CMS
   *************************************************
   *************************************************
   *************************************************
   */

  public class ElementValuesProvider
  {
    // default choice item if errors
    public static readonly SelectionChoiceItem ContactDev = new SelectionChoiceItem { Label = "contact developer", Value = "contact developer" };

    /// <summary>
    /// This function is a customization point for Enumeration and Multiselect CMS
    /// elements to provide options like data driven select lists etc.
    /// </summary>
    /// <param name="schemaName">Schema root name (i.e. Page Type)</param>
    /// <param name="elementName">Tag name of the element</param>
    /// <param name="options">object that is used both as input and
    /// output for this customizable function. Some options include information
    /// about the request, while others allow modifying how the response is processed.</param>
    /// <param name="site">the CSAPI Site Object, created in a special session</param>
    /// <returns>Collection of strings and field choices</returns>
    public IEnumerable<SelectionChoiceItem> GetElementValues(string schemaName, string elementName, ChoicesProviderOptions options, ISite site)
    {
      try
      {
        Dictionary<string, IEnumerable<string>> choiceItems = ParseSelectionChoiceItemsFromCmsAsset(site.Session, "a/128073");

        if (choiceItems.ContainsKey(elementName))
        {
          List<SelectionChoiceItem> selectionChoiceItems = new List<SelectionChoiceItem>();

          foreach (string value in choiceItems[elementName])
          {
            selectionChoiceItems.Add(new SelectionChoiceItem { Label = value, Value = value });
          }

          return selectionChoiceItems;
        }
        else
        {
          LoggerService.Error($"Element '{elementName}' not found in schema '{schemaName}'");
        }
      }
      catch (Exception e)
      {
        LoggerService.Error($"Error retrieving element values: {e.Message}");
      }

      return new SelectionChoiceItem[] { ContactDev };
    }

    public IEnumerable<SourcedSelectionChoiceItem> GetDITAMapDetailFieldsCustom(IUserSession session, bool includeValue)
    {
      return Enumerable.Empty<SourcedSelectionChoiceItem>();
    }

    private static Dictionary<string, IEnumerable<string>> ParseSelectionChoiceItemsFromCmsAsset(IUserSession session, string id)
    {


      Dictionary<string, IEnumerable<string>> container;


      IAsset asset = session.Site.Asset(id);

      if (asset == null)
      {
        throw new ArgumentException($"Asset with ID: {id} does not exist");
      }

      string fullPath = asset.File().FullName;
      string serialized = System.IO.File.ReadAllText(fullPath);

      container = JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<string>>>(serialized);


      return container;
    }
  }

  public class LoggerService
  {
    private static ISite _site;

    public static void Error(Exception ex)
    {
      if (_site == null)
        return;

      _site.Session.Error($"{GetExceptionFileAndLine(ex)} - {ex.Message}", ex);
    }

    public static void Error(string message)
    {
      if (_site == null)
        return;

      _site.Session.Error(message);
    }

    private static string GetExceptionFileAndLine(Exception ex)
    {
      var stackWithFileInfo = new StackTrace(ex, true);
      StackFrame firstFrame = stackWithFileInfo.GetFrame(0);
      string fileName = firstFrame.GetFileName();
      int lineNumber = firstFrame.GetFileLineNumber();

      return $"[{fileName}, line {lineNumber}]";
    }

    /// <summary>
    /// Sets the site to access Session Logger
    /// </summary>
    /// <param name="site"></param>
    public static void SetSite(ISite site)
    {
      _site = site;
    }
  }
}