using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Eppei.Crm.Manager
{
    public class ApplicantManager
    {

        public static Entity GetApplicantWithAllAttributes(IOrganizationService service, Guid applicantId)
        {
            var applicant = Qe.QueryRecord(service, "eppei_applicant", applicantId, true);
            return applicant;
        }

        public static Entity GetApplicant(IOrganizationService service, Guid applicantId, string[] columns)
        {
            var applicant = Qe.QueryRecord(service, "eppei_applicant", applicantId, columns);
            return applicant;
        }

    }
}
