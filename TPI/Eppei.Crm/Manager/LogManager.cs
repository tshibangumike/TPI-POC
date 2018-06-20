using Microsoft.Xrm.Sdk;

namespace Eppei.Crm.Manager
{
    public sealed class LogManager
    {

        public LogManager() { }

        public static void CreateLog(IOrganizationService service, string title, string message, int type)
        {
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(message) && type == 0) return;
            var log = new Entity("eppei_log")
            {
                ["eppei_name"] = title,
                ["eppei_message"] = message,
                ["eppei_type"] = new OptionSetValue(type)
            };
            service.Create(log);
        }

        public static void LogMissingAttributeMessage(IOrganizationService service, string entityName, string where, string[] attributes)
        {
            if (attributes == null || attributes.Length <= 0) return;
            var attributeString = string.Join(",", attributes);
            var message = string.Format("{0} attribute(s) does(do) not have value");
            var log = new Entity("eppei_log")
            {
                ["eppei_name"] = "Missing Attribute Value in: " + where,
                ["eppei_message"] = message,
                ["eppei_type"] = new OptionSetValue(2)
            };
            service.Create(log);
        }

    }
}
