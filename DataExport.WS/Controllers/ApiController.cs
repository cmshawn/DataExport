﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using Common.Logging;
using CtcApi;
using CtcApi.Extensions;
using CtcApi.Web.Mvc;
using DataExport.WS.Config;
using DataExport.Web.Properties;

namespace DataExport.WS.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  public class ApiController : BaseController
  {
    private ApplicationContext _appContext;
    private ILog _log = LogManager.GetCurrentClassLogger();

    private readonly DataExportConfig _config;
    private IList<Exporter> _exporters;


    #region Initialization
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This constructor initializes the <see cref="BaseController"/>, which provides common
    /// functionality like populating <i>ViewBag.Version</i> with the <see cref="Version"/>
    /// of the current MVC application.
    /// </remarks>
    public ApiController() : this(new ApplicationContext { BaseDirectory = AppDomain.CurrentDomain.BaseDirectory })
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This constructor initializes the <see cref="BaseController"/>, which provides common
    /// functionality like populating <i>ViewBag.Version</i> with the <see cref="Version"/>
    /// of the current MVC application.
    /// </remarks>
    public ApiController(ApplicationContext context) : base(Assembly.GetExecutingAssembly())
    {
      _appContext = context;
      _config = ConfigurationManager.GetSection(DataExportConfig.GetSectionName()) as DataExportConfig;
      _exporters = _config.Exporters;
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ActionResult Index()
    {
      _log.Trace(m => m("=> ApiController::Index()"));

      return View();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult Export(string id)
    {
      IEnumerable<Exporter> exporters = _exporters.Where(x => x.Name.ToUpper() == id.ToUpper());
      int exporterCount = exporters.Count();

      if (exporterCount > 0)
      {
        if (_log.IsWarnEnabled && exporterCount > 1)
        {
          _log.Warn(m => m("Multiple exporters found with the name '{0}' - only one will be used.", id));
        }

        Exporter exporter = exporters.Take(1).Single();
        exporter.Context = _appContext;

        string csv = exporter.Export();
        _log.Trace(m => m("Export result:\n{0}", csv));

        if (string.IsNullOrWhiteSpace(csv))
        {
          _log.Warn(m => m("Exporter '{0}' returned an empty string.", id));
        }
        else
        {
          // TODO: add config settings for saving file
          /***********************************************************************************************
           * WARNING: Only return file when testing. The downloaded contents could contain sensitive data.
           ***********************************************************************************************/
          if (Settings.Default.AllowFileDownload)
          {
            // Generated file is returned to the browser/client
            return new FileContentResult(Encoding.UTF8.GetBytes(csv), "text/plain");
          }
          return View("ExportResult", true);
        }
      }
      else
      {
        _log.Warn(m => m("No exporter was found for '{0}'", id));
      }
      return View("ExportResult", false);
    }
  }
}
