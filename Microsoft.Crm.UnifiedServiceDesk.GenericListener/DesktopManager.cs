using Microsoft.Uii.Common;
using Microsoft.Uii.Csr;
using Microsoft.Uii.Csr.Aif.HostedApplication;
using Microsoft.Uii.Desktop.Cti.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using Logic;
using Microsoft.Xrm.Sdk;
using Common;

namespace Microsoft.Crm.UnifiedServiceDesk.GenericListener
{
	public class DesktopManager : HostedWpfControl
	{
		private HttpListener crmListener;

		private Thread crmRequestWorker;

		public DesktopManager()
		{
		}

		public DesktopManager(Guid appID, string appName, string initString)
			: base(appID, appName, initString)
		{
		}

		public override void Initialize()
		{
			base.Initialize();
			crmListener = new HttpListener();
			string text = ConfigurationValueReader.ConfigurationReader().ReadAppSettings("GenericListener");
			if (string.IsNullOrEmpty(text))
			{
				text = "http://localhost:5000/";
			}
			crmListener.Prefixes.Add(text);
			crmListener.Start();
			crmRequestWorker = new Thread(RequestWorker);
			crmRequestWorker.IsBackground = true;
			crmRequestWorker.Start();
			PerfLogger.LogData("UII_HOSTED_APP_LOAD_" + GetHashCode(), new Dictionary<string, string>
			{
				{
					"AppID",
					base.ApplicationID.ToString()
				},
				{
					"Name",
					base.ApplicationName
				}
			});
		}
        //The generic listener will listen on this method when a url is hit. You can write or handle your custom
        //logic from this code. When a screen pop happens from the CTI clients (finesse, InContact etc.), this
        //method is triggered.
		private void RequestWorker(object obj)
		{
			while (true)
			{
				HttpListenerContext context = crmListener.GetContext();
				string query = "";
				using (StreamReader streamReader = new StreamReader(context.Request.InputStream))
				{
					query = streamReader.ReadToEnd();
				}
				string ani = string.Empty;
				string dnis = string.Empty;				
				string callType = string.Empty;
				string direction = string.Empty;
				List<LookupRequestItem> list = new List<LookupRequestItem>();
				NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(query);
				nameValueCollection.Add(context.Request.QueryString);
				string[] allKeys = nameValueCollection.AllKeys;
				foreach (string text in allKeys)
				{
					if (!string.IsNullOrEmpty(text))
					{
						switch (text.ToLower())
						{
						case "ani":
							ani = nameValueCollection[text];
								//Custom Code - to remove the incoming phone number prefix "91" from the CISCO Finesse
								//if (ani.Length.Equals(12) && ani.Substring(0, 2).Equals("91"))
								if (ani.Substring(0, 2).Equals("91"))
								{
									ani = ani.Substring(2);
									Case incident = new Case();
                                    //We will pass the IncidentId and the CaseTitle to the CTI replacement parameters.
                                    //We can then use these replacement parameters within USD to perform different operations
                                    //like opening a case record and creating/opening a new phone call record.
									Entity entityCase = incident.GetCase(ani);
									if(entityCase != null)
									{
										string IncidentId = Convert.ToString(entityCase.Id);
										list.Add(new LookupRequestItem("IncidentId", IncidentId));
										if(entityCase.Contains("title"))
										{
											string caseTitle = Convert.ToString(entityCase.Attributes["title"]);
											list.Add(new LookupRequestItem("CaseTitle", caseTitle));
										}
									}
								}
							//
							break;
						case "dnis":
							dnis = nameValueCollection[text];
							break;
						case "type":
							callType = nameValueCollection[text];
							break;
						case "direction":
							direction = nameValueCollection[text];
							break;
						default:
							list.Add(new LookupRequestItem(text, nameValueCollection[text]));
							break;
						}
					}
				}
				try
				{
					if (direction.Equals("outgoing"))
					{

					}
					else
					{
						using (StreamWriter streamWriter = new StreamWriter(context.Response.OutputStream))
						{
							context.Response.AddHeader("cache-control", "no-cache");
							context.Response.AddHeader("pragma", "no-cache");
							context.Response.AddHeader("expires", "0");
							context.Response.AddHeader("expiresabsolute", "-1");
							streamWriter.WriteLine("<html><head></head></html>");
							streamWriter.Close();
						}
						CtiLookupRequest ctiLookupRequest = new CtiLookupRequest(Guid.NewGuid(), base.ApplicationName, callType, ani, dnis);
						ctiLookupRequest.Items.AddRange(list);
						FireRequestAction(new RequestActionEventArgs("*", CtiLookupRequest.CTILOOKUPACTIONNAME, GeneralFunctions.Serialize(ctiLookupRequest)));
					}
				}
				catch
				{

				}
				try
				{
					//CtiLookupRequest ctiLookupRequest = new CtiLookupRequest(Guid.NewGuid(), base.ApplicationName, callType, ani, dnis);
					//ctiLookupRequest.Items.AddRange(list);
					//FireRequestAction(new RequestActionEventArgs("*", CtiLookupRequest.CTILOOKUPACTIONNAME, GeneralFunctions.Serialize(ctiLookupRequest)));
				}
				catch
				{
				}
			}
		}
	}
}
