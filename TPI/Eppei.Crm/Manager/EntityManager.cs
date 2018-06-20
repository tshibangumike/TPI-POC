using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eppei.Crm.Manager
{
    public sealed class EntityManager
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="EntityManager"/> class from being created.
        /// </summary>
        private EntityManager()
        {
        }

        /// <summary>
        /// Set the state of the entity
        /// </summary>
        /// <param name="service">The Service Context</param>
        /// <param name="entityMoniker">The entity</param>
        /// <param name="stateCode">The state code</param>
        /// <param name="statusCode">The status code</param>
        /// <returns>The SetStateResponse</returns>
        public static SetStateResponse SetState(IOrganizationService service, EntityReference entityMoniker, int stateCode, int statusCode)
        {
            SetStateRequest setStateRequest = new SetStateRequest
            {
                EntityMoniker = entityMoniker,
                State = new OptionSetValue(stateCode),
                Status = new OptionSetValue(statusCode)
            };
            return (SetStateResponse)service.Execute(setStateRequest);
        }

        /// <summary>
        /// Get the value of the specified entity attribute
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="attribute">The attribute</param>
        /// <returns>The specified attribute value from an entity</returns>
        public static object GetValue(Entity entity, string attribute)
        {
            return GetValue(entity, attribute, null);
        }

        /// <summary>
        /// Get the value of the specified entity attribute
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="attribute">The attribute</param>
        /// <param name="replace">The value to return if the specified attribute is null</param>
        /// <returns>The specified attribute value from an entity</returns>
        public static object GetValue(Entity entity, string attribute, object replace)
        {
            if (HasValue(entity, attribute))
            {
                return entity[attribute];
            }
            else
            {
                return replace;
            }
        }

        /// <summary>
        /// Set the value of the specified entity attribute
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="attribute">The attribute</param>
        /// <param name="value">The value</param>
        public static void SetValue(Entity entity, string attribute, object value)
        {
            entity[attribute] = value;
        }

        /// <summary>
        /// Set the value of the specified entity attribute if the entity attribute is null
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="attribute">The attribute</param>
        /// <param name="value">The value</param>
        public static void SetValueIfNull(Entity entity, string attribute, object value)
        {
            if (!HasValue(entity, attribute))
            {
                entity[attribute] = value;
            }
        }

        /// <summary>
        /// Checks whether a property of an entity has a value
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="attribute">The attribute</param>
        /// <returns>Whether a property of an entity has a value</returns>
        public static bool HasValue(Entity entity, string attribute)
        {
            if (entity.Attributes.Contains(attribute))
            {
                if (entity.Attributes[attribute] == null)
                {
                    return false;
                }
                else
                {
                    return !string.IsNullOrEmpty(entity.Attributes[attribute].ToString());
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a value indicating whether value of any of the supplied attributes have changed between two entity instances
        /// </summary>
        /// <param name="preEntity">The pre entity instance</param>
        /// <param name="postEntity">The post entity instance</param>
        /// <param name="attributes">The attributes to check</param>
        /// <returns>A value indicating whether value of any of the supplied attributes have changed between two entity instances</returns>
        public static bool HaveValuesChanged(Entity preEntity, Entity postEntity, params string[] attributes)
        {
            foreach (string attribute in attributes)
            {
                if (HasValueChanged(preEntity, postEntity, attribute))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a value indicating whether the value of an attribute has changed between two entity instances
        /// </summary>
        /// <param name="preEntity">The pre entity instance</param>
        /// <param name="postEntity">The post entity instance</param>
        /// <param name="attribute">The attribute to check</param>
        /// <returns>A value indicating whether the value of an attribute has changed between two entity instances</returns>
        public static bool HasValueChanged(Entity preEntity, Entity postEntity, string attribute)
        {
            if (postEntity.Contains(attribute))
            {
                if (HasValue(postEntity, attribute))
                {
                    if (HasValue(preEntity, attribute))
                    {
                        if (postEntity[attribute].GetType() == typeof(EntityReference))
                        {
                            return ((EntityReference)preEntity[attribute]).Id != ((EntityReference)postEntity[attribute]).Id;
                        }
                        else if (postEntity[attribute].GetType() == typeof(Money))
                        {
                            return ((Money)preEntity[attribute]).Value != ((Money)postEntity[attribute]).Value;
                        }
                        else if (postEntity[attribute].GetType() == typeof(OptionSetValue))
                        {
                            return ((OptionSetValue)preEntity[attribute]).Value != ((OptionSetValue)postEntity[attribute]).Value;
                        }
                        else if (postEntity[attribute].GetType() == typeof(DateTime))
                        {
                            return !((DateTime)preEntity[attribute]).Equals(postEntity[attribute]);
                        }
                        else if (postEntity[attribute].GetType() == typeof(Boolean))
                        {
                            return !((Boolean)preEntity[attribute]).Equals(postEntity[attribute]);
                        }
                        else if (postEntity[attribute].GetType() == typeof(EntityCollection))
                        {
                            EntityCollection entityCollectionPre = (EntityCollection)preEntity[attribute];
                            EntityCollection entityCollectionPost = (EntityCollection)postEntity[attribute];
                            if (entityCollectionPre.Entities.Count != entityCollectionPost.Entities.Count)
                            {
                                return true;
                            }
                            else
                            {
                                for (int i = 0; i < entityCollectionPre.Entities.Count; i++)
                                {
                                    Entity activityPartyPre = entityCollectionPre.Entities[i];
                                    Entity activityPartyPost = entityCollectionPost.Entities[i];

                                    if (EntityManager.HasValue(activityPartyPre, "partyid"))
                                    {
                                        if (!EntityManager.HasValue(activityPartyPost, "partyid") || ((EntityReference)activityPartyPre["partyid"]).Id != ((EntityReference)activityPartyPost["partyid"]).Id)
                                        {
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        if (EntityManager.HasValue(activityPartyPost, "partyid") || (string)activityPartyPre["addressused"] != (string)activityPartyPost["addressused"])
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            return preEntity[attribute] != postEntity[attribute];
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return HasValue(preEntity, attribute);
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the value of the supplied attribute from the first Entity reference where the value is not null
        /// </summary>
        /// <param name="attribute">The attribute</param>
        /// <param name="entities">The entity</param>
        /// <returns>The value of the supplied attribute from the first Entity reference where the value is not null</returns>
        public static object Coalesce(string attribute, params Entity[] entities)
        {
            foreach (Entity entity in entities)
            {
                if (EntityManager.HasValue(entity, attribute))
                {
                    return entity[attribute];
                }
            }
            return null;
        }

        /// <summary>
        /// Sets a value on a CRM Entity to the supplied value and returns a value indicating whether the value changed or not
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="attributeName">The attribute</param>
        /// <param name="newValue">The value to set the attribute to</param>
        /// <returns>A value indicating whether the value changed or not</returns>
        public static bool SetValue(Entity entity, string attributeName, string newValue)
        {
            if (newValue != null)
            {
                newValue = newValue.Trim();
            }
            if (entity.Attributes.Contains(attributeName))
            {
                if ((string)entity[attributeName] != newValue)
                {
                    entity[attributeName] = newValue;
                    return true;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(newValue))
                {
                    entity[attributeName] = newValue;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets a value on a CRM Entity to the supplied value and returns a value indicating whether the value changed or not
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="attributeName">The attribute</param>
        /// <param name="newValue">The value to set the attribute to</param>
        /// <returns>A value indicating whether the value changed or not</returns>
        public static bool SetValue(Entity entity, string attributeName, decimal? newValue)
        {
            return SetValue(entity, attributeName, newValue, false);
        }

        /// <summary>
        /// Sets a value on a CRM Entity to the supplied value and returns a value indicating whether the value changed or not
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="attributeName">The attribute</param>
        /// <param name="newValue">The value to set the attribute to</param>
        /// <param name="isMoney">Indicates whether or not the field is a money field. Defaults to false.</param>
        /// <returns>A value indicating whether the value changed or not</returns>
        public static bool SetValue(Entity entity, string attributeName, decimal? newValue, bool isMoney)
        {
            if (entity.Attributes.Contains(attributeName))
            {
                if (isMoney)
                {
                    if (newValue.HasValue)
                    {
                        if (((Money)entity[attributeName]).Value != newValue.Value)
                        {
                            entity[attributeName] = new Money(newValue.Value);
                            return true;
                        }
                    }
                    else
                    {
                        entity[attributeName] = null;
                        return true;
                    }
                }
                else
                {
                    if ((decimal?)entity[attributeName] != newValue)
                    {
                        entity[attributeName] = newValue;
                        return true;
                    }
                }
            }
            else
            {
                if (newValue.HasValue)
                {
                    if (isMoney)
                    {
                        if (newValue.HasValue)
                        {
                            entity[attributeName] = new Money(newValue.Value);
                        }
                        else
                        {
                            entity[attributeName] = null;
                        }
                    }
                    else
                    {
                        entity[attributeName] = newValue;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets a value on a CRM Entity to the supplied value and returns a value indicating whether the value changed or not
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="attributeName">The attribute</param>
        /// <param name="newValue">The value to set the attribute to</param>
        /// <returns>A value indicating whether the value changed or not</returns>
        public static bool SetValue(Entity entity, string attributeName, double? newValue)
        {
            if (entity.Attributes.Contains(attributeName))
            {
                if ((double?)entity[attributeName] != newValue)
                {
                    entity[attributeName] = newValue;
                    return true;
                }
            }
            else
            {
                if (newValue.HasValue)
                {
                    entity[attributeName] = newValue;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets a value on a CRM Entity to the supplied value and returns a value indicating whether the value changed or not
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="attributeName">The attribute</param>
        /// <param name="newValue">The value to set the attribute to</param>
        /// <returns>A value indicating whether the value changed or not</returns>
        public static bool SetValue(Entity entity, string attributeName, int? newValue)
        {
            return SetValue(entity, attributeName, newValue, false);
        }

        /// <summary>
        /// Sets a value on a CRM Entity to the supplied value and returns a value indicating whether the value changed or not
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="attributeName">The attribute</param>
        /// <param name="newValue">The value to set the attribute to</param>
        /// <param name="isOptionSet">A value indicating whether the item is an Option Set field. Defaults to false</param>
        /// <returns>A value indicating whether the value changed or not</returns>
        public static bool SetValue(Entity entity, string attributeName, int? newValue, bool isOptionSet)
        {
            if (entity.Attributes.Contains(attributeName))
            {
                if (isOptionSet)
                {
                    if (newValue.HasValue)
                    {
                        if (((OptionSetValue)entity[attributeName]).Value != newValue.Value)
                        {
                            entity[attributeName] = new OptionSetValue(newValue.Value);
                            return true;
                        }
                    }
                    else
                    {
                        entity[attributeName] = null;
                        return true;
                    }
                }
                else
                {
                    if ((int?)entity[attributeName] != newValue)
                    {
                        entity[attributeName] = newValue;
                        return true;
                    }
                }
            }
            else
            {
                if (newValue.HasValue)
                {
                    if (isOptionSet)
                    {
                        entity[attributeName] = new OptionSetValue(newValue.Value);
                    }
                    else
                    {
                        entity[attributeName] = newValue;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets a value on a CRM Entity to the supplied value and returns a value indicating whether the value changed or not
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="attributeName">The attribute</param>
        /// <param name="newValue">The value to set the attribute to</param>
        /// <returns>A value indicating whether the value changed or not</returns>
        public static bool SetValue(Entity entity, string attributeName, bool? newValue)
        {
            if (entity.Attributes.Contains(attributeName))
            {
                if ((bool?)entity[attributeName] != newValue)
                {
                    entity[attributeName] = newValue;
                    return true;
                }
            }
            else
            {
                if (newValue.HasValue)
                {
                    entity[attributeName] = newValue;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets a value on a CRM Entity to the supplied value and returns a value indicating whether the value changed or not
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="attributeName">The attribute</param>
        /// <param name="newValue">The value to set the attribute to</param>
        /// <param name="logicalName">The entity logical name</param>
        /// <returns>A value indicating whether the value changed or not</returns>
        public static bool SetValue(Entity entity, string attributeName, Guid? newValue, string logicalName)
        {
            if (entity.Attributes.Contains(attributeName))
            {
                if (newValue.HasValue)
                {
                    if (((EntityReference)entity[attributeName]).Id != newValue)
                    {
                        entity[attributeName] = new EntityReference(logicalName, newValue.Value);
                        return true;
                    }
                }
                else
                {
                    entity[attributeName] = null;
                    return true;
                }
            }
            else
            {
                if (newValue.HasValue)
                {
                    entity[attributeName] = new EntityReference(logicalName, newValue.Value);
                    return true;
                }
            }
            return false;
        }
    }
}
