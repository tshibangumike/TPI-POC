using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Eppei.Crm
{
    public class Qe
    {

        public static DataCollection<Entity> QueryMultipleRecords(IOrganizationService serviceContext, string entityName,
            string[] columns, ConditionExpression[] conditions)
        {
            var query = new QueryExpression(entityName);
            if (conditions != null)
            {
                var filter = new FilterExpression();
                foreach (var condition in conditions)
                {
                    filter.Conditions.Add(condition);
                }
                query.Criteria.AddFilter(filter);
            }
            if (columns != null)
                query.ColumnSet = new ColumnSet(columns);
            var result = serviceContext.RetrieveMultiple(query);
            return result.Entities;
        }

        public static DataCollection<Entity> QueryMultipleRecords(IOrganizationService serviceContext, string entityName,
           bool columns, ConditionExpression[] conditions)
        {
            var query = new QueryExpression(entityName);
            if (conditions != null)
            {
                var filter = new FilterExpression();
                foreach (var condition in conditions)
                {
                    filter.Conditions.Add(condition);
                }
                query.Criteria.AddFilter(filter);
            }
            query.ColumnSet = new ColumnSet(true);
            var result = serviceContext.RetrieveMultiple(query);
            return result.Entities;
        }

        public static Entity QueryRecord(IOrganizationService serviceContext, string entityName, string[] columns,
            ConditionExpression[] conditions)
        {
            var query = new QueryExpression(entityName) { TopCount = 1 };
            if (conditions != null)
            {
                var filter = new FilterExpression();
                foreach (var condition in conditions)
                {
                    filter.Conditions.Add(condition);
                }
                query.Criteria.AddFilter(filter);
            }
            if (columns != null)
                query.ColumnSet = new ColumnSet(columns);
            var result = serviceContext.RetrieveMultiple(query);
            if (result.Entities != null && result.Entities.Count > 0)
            {
                return result.Entities[0];
            }
            return null;
        }

        public static Entity QueryRecord(IOrganizationService serviceContext, string entityName, bool columns,
            ConditionExpression[] conditions)
        {
            var query = new QueryExpression(entityName) { TopCount = 1 };
            if (conditions != null)
            {
                var filter = new FilterExpression();
                foreach (var condition in conditions)
                {
                    filter.Conditions.Add(condition);
                }
                query.Criteria.AddFilter(filter);
            }
            query.ColumnSet = new ColumnSet(columns);
            var result = serviceContext.RetrieveMultiple(query);
            if (result.Entities != null && result.Entities.Count > 0)
            {
                return result.Entities[0];
            }
            return null;
        }

        public static Entity QueryRecord(IOrganizationService serviceContext, string entityName, Guid recordId, bool columns)
        {
            var result = serviceContext.Retrieve(entityName, recordId, new ColumnSet(columns));
            if (result != null)
                return result;
            return null;
        }

        public static Entity QueryRecord(IOrganizationService serviceContext, string entityName, Guid recordId, string[] columns)
        {
            var result = serviceContext.Retrieve(entityName, recordId, new ColumnSet(columns));
            if (result != null)
                return result;
            return null;
        }

    }
}
