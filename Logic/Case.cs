using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Logic
{
    public class Case
    {
        IOrganizationService organizationService = null;
        public Entity GetCase(string ani)
        {
            Entity contact = null;
            Entity incident = null;
            try
            {
                Helper helper = new Helper();
                organizationService = helper.Authentication();

                QueryExpression queryContact = new QueryExpression("contact");
                queryContact.ColumnSet = new ColumnSet(true);
                ConditionExpression conditionMobile = new ConditionExpression("mobilephone", ConditionOperator.Equal, ani);
                queryContact.Criteria.AddCondition(conditionMobile);
                
                if (organizationService != null)
                {
                    EntityCollection entityContact = organizationService.RetrieveMultiple(queryContact);
                    if (entityContact != null && entityContact.Entities.Count == 1)
                    {
                        foreach (var cont in entityContact.Entities)
                        {
                            contact = cont;
                            break;
                        }
                        if (contact != null)
                        {
                            incident = GetCase(contact);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return incident;
        }

        public Entity GetCase(Entity contact)
        {
            Entity Case = null;
            QueryExpression queryCase = new QueryExpression("incident");
            queryCase.ColumnSet = new ColumnSet("title");
            ConditionExpression conditionCustomer = new ConditionExpression("customerid", ConditionOperator.Equal, contact.Id);
            ConditionExpression conditionStatus = new ConditionExpression("statecode", ConditionOperator.Equal, 0);
            queryCase.Criteria.AddCondition(conditionCustomer);
            queryCase.Criteria.AddCondition(conditionStatus);
            EntityCollection entityContact = organizationService.RetrieveMultiple(queryCase);
            if (entityContact != null && entityContact.Entities.Count == 1)
            {
                foreach (var ent in entityContact.Entities)
                {
                    Case = ent;
                    break;
                }
            }
            return Case;
        }
    }
}
