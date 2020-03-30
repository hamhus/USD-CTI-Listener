using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Helper
    {
        IOrganizationService organizationService = null;
        public IOrganizationService Authentication()
        {
            try
            {
                ClientCredentials clientCredentials = new ClientCredentials();
                clientCredentials.UserName.UserName = ""; //Email Address Here
                clientCredentials.UserName.Password = ""; // Password Here
                string crmOrg = "https://xxx.api.crm.dynamics.com/XRMServices/2011/Organization.svc"; //CRM org here 
                
                // For Dynamics 365 Customer Engagement V9.X, set Security Protocol as TLS12
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Get the URL from CRM, Navigate to Settings -> Customizations -> Developer Resources
                // Copy and Paste Organization Service Endpoint Address URL
                organizationService = new OrganizationServiceProxy(new Uri(crmOrg), //fill in url details
                 null, clientCredentials, null);
                //CrmSer
                if (organizationService != null)
                {
                    Guid userid = ((WhoAmIResponse)organizationService.Execute(new WhoAmIRequest())).UserId;
                    if (userid != Guid.Empty)
                    {
                        

                    }
                }
                else
                {
                    Console.WriteLine("Failed to Established Connection!!!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught - " + ex.Message);
            }
            return organizationService;
        }
    }
}
