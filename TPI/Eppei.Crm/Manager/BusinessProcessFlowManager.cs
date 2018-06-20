using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Eppei.Crm.Manager
{
    public class BusinessProcessFlowManager
    {
        public static Entity GetBpfApplicantRecordRelatedToApplicant(IOrganizationService service, Entity applicant)
        {
            /*Applicant's Attributes Validation*/
            if (!applicant.Attributes.Contains("eppei_iseskom") || !applicant.Attributes.Contains("eppei_academicdegreeid")) return null;
            /*Query Qualification*/
            var qualificationEr = (EntityReference)applicant.Attributes["eppei_academicdegreeid"];
            var isEskom = (bool)applicant.Attributes["eppei_iseskom"];
            /*Query BPF Applicant Record*/
            var columns = new [] { "eppei_businessprocessflowid", "eppei_bbfapplicantid", "eppei_bpfentityname", "eppei_iseskom", "eppei_qualificationid" };
            var conditions = new[] { new ConditionExpression("eppei_iseskom", ConditionOperator.Equal, isEskom),
            new ConditionExpression("eppei_qualificationid", ConditionOperator.Equal, qualificationEr.Id)};
            var bpfApplicantRecord = Qe.QueryRecord(service, "eppei_bbfapplicant", columns, conditions);
            return bpfApplicantRecord;
        }

        public static Entity GetBusinessProcessFlowInstanceRelatedToApplicant(IOrganizationService service, string bpfEntityName, Guid applicantId, Guid processId)
        {
            var columns = new [] { "bpf_eppei_applicantid", "processid", "traversedpath", "activestageid" };
            var conditions = new[] {
                new ConditionExpression("bpf_eppei_applicantid", ConditionOperator.Equal, applicantId),
                new ConditionExpression("processid", ConditionOperator.Equal, processId)};
            var bpfInstanceRecord = Qe.QueryRecord(service, bpfEntityName, columns, conditions);
            return bpfInstanceRecord;
        }

        public static DataCollection<Entity> GetProcessStages(IOrganizationService service, Guid processId, string[] columns)
        {
            var conditions = new[] { new ConditionExpression("processid", ConditionOperator.Equal, processId) };
            var processStages = Qe.QueryMultipleRecords(service, "processstage", columns, conditions);
            return processStages;
        }
    }
}
