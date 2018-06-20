using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tpi.ConsoleApp
{
    public class Setup
    {

        private static string publisherPrefix = "blu_";
        private const int _languageCode = 1033;

        #region Create Attribute Requests
        private static BooleanAttributeMetadata CreateBooleanAttribute(string name, string displayName, AttributeRequiredLevel attributeRequiredLevel, string desription)
        {
            // Create a boolean attribute
            BooleanAttributeMetadata boolAttribute = new BooleanAttributeMetadata
            {
                // Set base properties
                SchemaName = publisherPrefix + name,
                LogicalName = publisherPrefix + name,
                DisplayName = new Label(displayName, _languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attributeRequiredLevel),
                Description = new Label(desription, _languageCode),
                // Set extended properties
                OptionSet = new BooleanOptionSetMetadata(
                    new OptionMetadata(new Label("True", _languageCode), 1),
                    new OptionMetadata(new Label("False", _languageCode), 0)
                    )
            };
            return boolAttribute;
        }

        private static DateTimeAttributeMetadata CreateDateTimeAttribute(string name, string displayName, AttributeRequiredLevel attributeRequiredLevel, string desription)
        {
            // Create a date time attribute
            DateTimeAttributeMetadata dtAttribute = new DateTimeAttributeMetadata
            {
                // Set base properties
                SchemaName = publisherPrefix + name,
                LogicalName = publisherPrefix + name,
                DisplayName = new Label(displayName, _languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attributeRequiredLevel),
                Description = new Label(desription, _languageCode),
                // Set extended properties
                Format = DateTimeFormat.DateOnly,
                ImeMode = ImeMode.Disabled,
            };
            return dtAttribute;
        }

        private static DecimalAttributeMetadata CreateDecimalAttributeMetadata(string name, string displayName, AttributeRequiredLevel attributeRequiredLevel, string desription, int minValue, int maxValue, int precision)
        {
            // Create a date time attribute
            DecimalAttributeMetadata decimalAttribute = new DecimalAttributeMetadata
            {
                // Set base properties
                SchemaName = publisherPrefix + name,
                LogicalName = publisherPrefix + name,
                DisplayName = new Label(displayName, _languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attributeRequiredLevel),
                Description = new Label(desription, _languageCode),
                // Set extended properties
                MaxValue = maxValue,
                MinValue = minValue,
                Precision = precision
            };
            return decimalAttribute;
        }

        private static IntegerAttributeMetadata CreateIntegerAttributeMetadata(string name, string displayName, AttributeRequiredLevel attributeRequiredLevel, string desription, int minValue, int maxValue)
        {
            // Create a date time attribute
            IntegerAttributeMetadata integerAttribute = new IntegerAttributeMetadata
            {
                // Set base properties
                SchemaName = publisherPrefix + name,
                LogicalName = publisherPrefix + name,
                DisplayName = new Label(displayName, _languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attributeRequiredLevel),
                Description = new Label(desription, _languageCode),
                // Set extended properties
                Format = IntegerFormat.None,
                MaxValue = maxValue,
                MinValue = minValue
            };
            return integerAttribute;
        }

        private static MoneyAttributeMetadata CreateMoneyAttribute(string name, string displayName, AttributeRequiredLevel attributeRequiredLevel, string desription, double minValue, double maxValue, int precision, int precisionSource)
        {
            // Create a date time attribute
            MoneyAttributeMetadata moneyAttribute = new MoneyAttributeMetadata
            {
                // Set base properties
                SchemaName = publisherPrefix + name,
                LogicalName = publisherPrefix + name,
                DisplayName = new Label(displayName, _languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attributeRequiredLevel),
                Description = new Label(desription, _languageCode),
                // Set extended properties
                MaxValue = maxValue,
                MinValue = minValue,
                Precision = precision,
                PrecisionSource = precisionSource,
                ImeMode = ImeMode.Disabled
            };
            return moneyAttribute;
        }

        private static StringAttributeMetadata CreateStringAttribute(string name, string displayName, AttributeRequiredLevel attributeRequiredLevel, string desription, int maxLength, StringFormat format = StringFormat.Text)
        {
            // Create a boolean attribute
            StringAttributeMetadata stringAttribute = new StringAttributeMetadata
            {
                // Set base properties
                SchemaName = publisherPrefix + name,
                LogicalName = publisherPrefix + name,
                DisplayName = new Label(displayName, _languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attributeRequiredLevel),
                Description = new Label(desription, _languageCode),
                Format = format,
                // Set extended properties
                MaxLength = maxLength
            };
            return stringAttribute;
        }

        private static MemoAttributeMetadata CreateMemoAttribute(string name, string displayName, AttributeRequiredLevel attributeRequiredLevel, string desription, int maxLength)
        {
            // Create a memo attribute 
            MemoAttributeMetadata memoAttribute = new MemoAttributeMetadata
            {
                // Set base properties
                SchemaName = publisherPrefix + name,
                LogicalName = publisherPrefix + name,
                DisplayName = new Label(displayName, _languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                Description = new Label(desription, _languageCode),
                // Set extended properties
                Format = StringFormat.TextArea,
                ImeMode = ImeMode.Disabled,
                MaxLength = maxLength
            };
            return memoAttribute;
        }

        private static PicklistAttributeMetadata CreateGlobalPickListAttribute(string globalOptionSetName, string name, string displayName, AttributeRequiredLevel attributeRequiredLevel, string desription)
        {
            PicklistAttributeMetadata pickListAttribute = new PicklistAttributeMetadata
            {
                // Set base properties
                SchemaName = publisherPrefix + name,
                LogicalName = publisherPrefix + name,
                DisplayName = new Label(displayName, _languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attributeRequiredLevel),
                Description = new Label(desription, _languageCode),
                // Set extended properties
                // In order to relate the picklist to the global option set, be sure
                // to specify the two attributes below appropriately.
                // Failing to do so will lead to errors.
                OptionSet = new OptionSetMetadata
                {
                    IsGlobal = true,
                    Name = publisherPrefix + globalOptionSetName
                }
            };
            return pickListAttribute;
        }

        private static void CreateOneToManyAttribute(OrganizationServiceProxy service, string referencedEntityName, string referencingEntityName, string referencedEntityDisplayName,
            string attributeSchemaName, string attributeDisplayName, string attributeDescription)
        {
            CreateOneToManyRequest createOneToManyRelationshipRequest = new CreateOneToManyRequest
            {
                OneToManyRelationship =
                new OneToManyRelationshipMetadata
                {
                    ReferencedEntity = referencingEntityName,
                    ReferencingEntity = referencedEntityName,
                    SchemaName = publisherPrefix + referencedEntityName + "_" + referencingEntityName + "_" + attributeSchemaName,
                    AssociatedMenuConfiguration = new AssociatedMenuConfiguration
                    {
                        Behavior = AssociatedMenuBehavior.UseLabel,
                        Group = AssociatedMenuGroup.Details,
                        Label = new Label(referencedEntityDisplayName, 1033),
                        Order = 10000
                    },
                    CascadeConfiguration = new CascadeConfiguration
                    {
                        Assign = CascadeType.NoCascade,
                        Delete = CascadeType.RemoveLink,
                        Merge = CascadeType.NoCascade,
                        Reparent = CascadeType.NoCascade,
                        Share = CascadeType.NoCascade,
                        Unshare = CascadeType.NoCascade
                    }
                },
                Lookup = new LookupAttributeMetadata
                {
                    SchemaName = publisherPrefix + attributeSchemaName,
                    DisplayName = new Label(attributeDisplayName, 1033),
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                    Description = new Label(attributeDescription, 1033)
                }
            };

            CreateOneToManyResponse createOneToManyRelationshipResponse =
                (CreateOneToManyResponse)service.Execute(
                createOneToManyRelationshipRequest);

            Console.WriteLine(
                "The one-to-many relationship has been created between {0} and {1}.",
                referencedEntityName, referencingEntityName);

        }
        #endregion

        #region Create Metadata

        public static void CreateMember(OrganizationServiceProxy service, string firstname, string lastname, string idnumber, string mobilephone, string telephone, string emailaddress, string countryofrigin, int gender, int race, string jobtitle, string areaofexpertise)
        {

            var member = new Entity("contact")
            {
                ["firstname"] = firstname,
                ["lastname"] = lastname,

                ["emailaddress1"] = emailaddress,
                ["mobilephone"] = mobilephone,
                ["address1_telephone2"] = telephone,
                ["eppei_countryoforigin"] = countryofrigin,
                ["eppei_gender"] = new OptionSetValue(gender),
                ["eppei_race"] = new OptionSetValue(race),
                ["jobtitle"] = jobtitle,
                ["eppei_membertype"] = new OptionSetValue(2),
                ["eppei_areaofindustrialexpertise"] = areaofexpertise
            };

            if (string.IsNullOrEmpty(idnumber))
                member["eppei_saidnumber"] = idnumber;

            service.Create(member);

            Console.WriteLine("Created: " + firstname + " " + lastname);

        }

        public static void CreateQuestionMapping(OrganizationServiceProxy service, string name, string type, string values, string desc)
        {

            var nameLength = name.Length > 40 ? 40 : name.Length;
            var uniqueName = name.Substring(0, nameLength);
            uniqueName = uniqueName.Replace("(", "").Replace(")", "").Replace("(", "").Replace("/", "")
                .Replace("-", "").Replace(";", "").Replace("?", "").Replace("&", "").Replace(",", "").Replace(":", "").Replace(" ", "").ToLower();

            var displaynameLength = name.Length > 50 ? 50 : name.Length;
            var DisplayName = name.Substring(0, displaynameLength);


            if (type == "Option Set")
            {

                var _values = values.Split(',');
                // Define the option set to create.
                OptionSetMetadata setupOptionSetMetadata = new OptionSetMetadata()
                {
                    // The name will be used to uniquely identify the option set.
                    // Normally you should generate this identifier using the publisher's
                    // prefix and double-check that the name is not in use.
                    Name = publisherPrefix + uniqueName,
                    DisplayName = new Label(DisplayName, _languageCode),
                    Description = new Label(desc, _languageCode),
                    IsGlobal = true,
                    OptionSetType = OptionSetType.Picklist,

                    // Define the list of options that populate the option set
                    // The order here determines the order shown in the option set.
                    Options =
                    {
                        // Options accepts any number of OptionMetadata instances, which
                        // are simply pairs of Labels and integer values.
                        new OptionMetadata(new Label(_values[0],_languageCode), null),
                        new OptionMetadata(new Label(_values[1],_languageCode), null),
                    }

                };

                // Wrap the OptionSetMetadata in the appropriate request.
                CreateOptionSetRequest createOptionSetRequest = new CreateOptionSetRequest
                {
                    OptionSet = setupOptionSetMetadata
                };

                // Pass the execute statement to the CRM service.
                //service.Execute(createOptionSetRequest);

                Console.WriteLine(DisplayName + " Option Set created");
                service.Execute(createOptionSetRequest);

            }
            else if (type.StartsWith("Mutli"))
            {
                var _values = values.Split(',');
                var outDoorActivitiesAttribute = new MultiSelectPicklistAttributeMetadata()
                {
                    SchemaName = publisherPrefix + uniqueName,
                    LogicalName = publisherPrefix + uniqueName,
                    DisplayName = new Label(DisplayName, _languageCode),
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                    Description = new Label(desc, _languageCode),
                    OptionSet = new OptionSetMetadata()
                    {
                        IsGlobal = false,
                        OptionSetType = OptionSetType.Picklist,
                        Options = {
                                new OptionMetadata(new Label(_values[0],_languageCode),1),
                                new OptionMetadata(new Label(_values[1],_languageCode),2)
                        }
                    }
                };

                CreateAttributeRequest createAttributeRequest = new CreateAttributeRequest
                {
                    EntityName = "blu_questionmapping",
                    Attribute = outDoorActivitiesAttribute
                };

                service.Execute(createAttributeRequest);
                Console.WriteLine(DisplayName + " Option Set created");
                

            }
            else if (type == "Boolean")
            {
                var att = CreateBooleanAttribute(uniqueName, DisplayName, AttributeRequiredLevel.None, desc);
                CreateAttributeRequest createAttributeRequest = new CreateAttributeRequest
                {
                    EntityName = "blu_questionmapping",
                    Attribute = att
                };
                service.Execute(createAttributeRequest);
            }
            else if (type == "Date & Time")
            {
                var att = CreateDateTimeAttribute(uniqueName, DisplayName, AttributeRequiredLevel.None, desc);
                CreateAttributeRequest createAttributeRequest = new CreateAttributeRequest
                {
                    EntityName = "blu_questionmapping",
                    Attribute = att
                };
                service.Execute(createAttributeRequest);
            }
            else if (type == "Multi Text")
            {
                var att = CreateMemoAttribute(uniqueName, DisplayName, AttributeRequiredLevel.None, desc, 4000);
                CreateAttributeRequest createAttributeRequest = new CreateAttributeRequest
                {
                    EntityName = "blu_questionmapping",
                    Attribute = att
                };
                service.Execute(createAttributeRequest);
            }
            else if (type == "Text")
            {
                var att = CreateStringAttribute(uniqueName, DisplayName, AttributeRequiredLevel.None, desc, 500);
                CreateAttributeRequest createAttributeRequest = new CreateAttributeRequest
                {
                    EntityName = "blu_questionmapping",
                    Attribute = att
                };
                service.Execute(createAttributeRequest);
            }
            else if (type == "Whole Integer")
            {
                var att = CreateIntegerAttributeMetadata(uniqueName, DisplayName, AttributeRequiredLevel.None, desc, -1000000, 1000000);
                CreateAttributeRequest createAttributeRequest = new CreateAttributeRequest
                {
                    EntityName = "blu_questionmapping",
                    Attribute = att
                };
                service.Execute(createAttributeRequest);
            }

        }

        public static void ReadCsv(OrganizationServiceProxy service)
        {

            using (var readFile = new StreamReader(@"C:\_WORK\Clients\Tpi\QuestionMapping.csv"))
            {
                string line;
                string[] row;
                while ((line = readFile.ReadLine()) != null)
                {
                    try
                    {
                        row = line.Split(';');
                        var name = row[0];
                        var type = row[1];
                        var values = row[2];
                        var desc = row[3];


                        CreateQuestionMapping(service, name, type, values, desc);

                        Console.WriteLine("name:" + name);
                        Console.WriteLine("type:" + type);
                        Console.WriteLine("values:" + values);
                        Console.WriteLine("desc:" + desc);
                        Console.WriteLine("--------------------------");

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        continue;

                    }


                }
            }

        }

        #endregion

    }
}
