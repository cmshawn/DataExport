﻿using System;
using System.Data;
using System.Text;
using CtcApi;

namespace DataExport.WS
{
	public class CsvFormat : CsvFormatConfig, IExportFormat
	{
		public ApplicationContext Context {get;set;}

		public string Serialize(DataSet ds)
		{
			StringBuilder csv = new StringBuilder();

			// TODO: are column headings required?

			// Add rows
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
				{
					if (i != 0)	// don't leave a trailing separator
					{
						csv.Append(FieldSeparator);
					}
					csv.Append(row[i].ToString().TrimEnd(FieldTrimEndChars).TrimStart(FieldTrimLeadingChars));
				}
				csv.Append(Environment.NewLine);
			}

			return csv.ToString();
		}
	}
}