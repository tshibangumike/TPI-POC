using System;

namespace tpi.Model
{
    public class InspectionPortalQA
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UniqueName { get; set; }
        public int AnswerDataType { get; set; }
        public string AnswerDataTypeText { get; set; }
        public int Number { get; set; }
        public string[] OptionSetValues { get; set; }
        public string Answer { get; set; }
        public Guid InspectionId { get; set; }
        public int Type { get; set; }
        public string UniqueQuestionName { get; set; }
        public Guid OrderId { get; set; }
        public Guid OpportunityId { get; set; }
        public Guid QuestionSetupId { get; set; }
        public Guid PropertyId { get; set; }
        public Guid AdditionalProductId { get; set; }
        public bool IsMandatory { get; set; }
    }

    public enum InspectionPortalQA_RequiredFor
    {
        Portal = 858890000,
        InspectorsApp = 858890001
    }

    public enum InspectionPortalQA_Type
    {
        Question = 858890001,
        TC = 858890002
    }

}
