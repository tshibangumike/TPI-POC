using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tpi.Model;

namespace tpi.Service
{
    public class AppointmentService
    {

        public static string EntityPluralName = "appointments";
        public static string ClassName = "AppointmentService";
        public static string webApiQueryUrl = ConfigurationManager.AppSettings["WebApiQueryUrl"];

        public static Appointment QueryAppointment(HttpClient httpClient, Guid appointmentId)
        {

            try
            {
                var odataQuery =
                    webApiQueryUrl + EntityPluralName +
                    "?$select=activityid,_ownerid_value,scheduledend,scheduledstart&$filter=statecode eq 0 and  activityid eq " +
                    appointmentId;

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());

                foreach (var data in systemUserObject.value)
                    return new Appointment()
                    {
                        Id = Guid.Parse(data["activityid"].ToString()),
                        StartTime = DateTime.Parse(data["scheduledstart"].ToString()),
                        StartTimeDisplayName = data["scheduledstart@OData.Community.Display.V1.FormattedValue"],
                        EndTime = DateTime.Parse(data["scheduledend"].ToString()),
                        EndTimeDisplayName = data["scheduledend@OData.Community.Display.V1.FormattedValue"],
                        OwnerId = Guid.Parse(data["_ownerid_value"].ToString()),
                        Owner = new SystemUser()
                        {
                            Id = Guid.Parse(data["_ownerid_value"].ToString()),
                            Fullname = data["_ownerid_value@OData.Community.Display.V1.FormattedValue"]
                        }
                    };

                return null;

            }
            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = ClassName + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return null;
            }

        }

        public static Guid CreateAppointment(OrganizationServiceProxy service, Appointment appointment)
        {
            try
            {

                
                var appointmentEntity = new Entity("appointment")
                {
                    ["subject"] = appointment.Subject.Length > 200 ? appointment.Subject.Substring(0, 200) : appointment.Subject,
                    ["scheduledend"] = appointment.EndTime,
                    ["scheduledstart"] = appointment.StartTime,
                    ["ownerid"] = new EntityReference("systemuser", appointment.OwnerId),
                    ["prioritycode"] = new OptionSetValue((appointment.PriorityCode == 2 ? 2 : 1)),
                    ["blu_createdfromportal"] = true
                };
                if (appointment.RegardingObjectId != Guid.Empty)
                {
                    appointmentEntity["regardingobjectid"] = new EntityReference("salesorder", appointment.RegardingObjectId);
                }
                return service.Create(appointmentEntity);
            }
            catch(Exception ex)
            {
                LogService.LogMessage(service, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = ClassName + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return Guid.Empty;
            }
        }

        public static Guid CreateAppointment(HttpClient httpClient, OrganizationServiceProxy service, Appointment appointment)
        {

            try
            {

                var hourDifference = DateTime.Now.Hour - DateTime.UtcNow.Hour;

                appointment.StartTime = new DateTime(appointment.StartTime.Year, appointment.StartTime.Month, appointment.StartTime.Day, (appointment.StartTime.Hour - hourDifference), 0, 0);
                appointment.EndTime = new DateTime(appointment.EndTime.Year, appointment.EndTime.Month, appointment.EndTime.Day, (appointment.EndTime.Hour - hourDifference), 0, 0);

                var appointmentObject = new JObject()
                {
                    ["subject"] = appointment.Subject.Length > 200 ? appointment.Subject.Substring(0, 200) : appointment.Subject,
                    ["scheduledstart"] = DateTime.SpecifyKind(appointment.StartTime, DateTimeKind.Utc),
                    ["scheduledend"] = DateTime.SpecifyKind(appointment.EndTime, DateTimeKind.Utc),
                    ["prioritycode"] = appointment.PriorityCode == 2 ? 2 : 1,
                    ["blu_createdfromportal"] = true
                };

                if(appointment.OwnerId != Guid.Empty)
                {
                    appointmentObject["ownerid@odata.bind"] = "/systemusers(" + appointment.OwnerId + ")";
                }

                if (appointment.RegardingObjectId != Guid.Empty)
                {
                    appointmentObject["regardingobjectid_salesorder@odata.bind"] = "/salesorders(" + appointment.RegardingObjectId + ")";
                }

                var request = new HttpRequestMessage(HttpMethod.Post,
                    webApiQueryUrl + EntityPluralName)
                {
                    Content = new StringContent(appointmentObject.ToString(), Encoding.UTF8, "application/json")
                };
                var response = httpClient.SendAsync(request);
                if (!response.Result.IsSuccessStatusCode)
                {
                    var error = response.Result.Content.ReadAsStringAsync();
                    LogService.LogMessage(httpClient, new Log()
                    {
                        Level = (int)LogType.Error,
                        Name = "Query Error",
                        FunctionName = ClassName + " | " + MethodBase.GetCurrentMethod().Name,
                        Message = error.Result.ToString()
                    });
                    return Guid.Empty;
                }
                var recordUrl = response.Result.Headers.GetValues("OData-EntityId").FirstOrDefault();
                if (recordUrl == null) return Guid.Empty;
                var splitRetrievedData = recordUrl.Split('[', '(', ')', ']');
                var recordId = Guid.Parse(splitRetrievedData[1]);
                return recordId;

            }
            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = ClassName + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return Guid.Empty;
            }

        }

        public static List<Appointment> QueryInspectorsAppointments(HttpClient httpClient, int numberOfInspectors)
        {

            var bookedAppointments = new List<Appointment>();
            var appointments = new List<Appointment>();
            var appointmentDates = new List<DateTime>();

            try
            {

                var fetchXML = string.Empty;
                fetchXML += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                fetchXML += "  <entity name='appointment'>";
                fetchXML += "    <attribute name='statecode' />";
                fetchXML += "    <attribute name='scheduledstart' />";
                fetchXML += "    <attribute name='scheduledend' />";
                fetchXML += "    <attribute name='ownerid' />";
                fetchXML += "    <attribute name='activityid' />";
                fetchXML += "    <filter type='and'>";
                fetchXML += "      <condition attribute='statecode' operator='in'>";
                fetchXML += "        <value>0</value>";
                fetchXML += "        <value>3</value>";
                fetchXML += "      </condition>";
                fetchXML += "      <condition attribute='blu_approvalstatus' operator='eq' value='" + (int)ApprovalStatus.New + "' />";
                fetchXML += "      <condition attribute='scheduledstart' operator='on-or-after' value='" + "2018-05-12" + "' />";
                fetchXML += "    </filter>";
                fetchXML += "  </entity>";
                fetchXML += "</fetch>";

                var encodedQuery = SharedService.UrlEncode(fetchXML);
                var odataQuery = webApiQueryUrl + "appointments?fetchXml=" +
                                 encodedQuery;

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());

                foreach (var data in systemUserObject.value)
                {
                    DateTime startTime = DateTime.Parse(data["scheduledstart"].ToString());
                    DateTime endTime = DateTime.Parse(data["scheduledend"].ToString());

                    var doesDateAlreadyExist = appointmentDates.Any(x => x.Date == startTime.Date);
                    if (!doesDateAlreadyExist)
                    {
                        appointmentDates.Add(startTime);
                    }

                    var appointment = new Appointment()
                    {
                        Id = data["activityid"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["activityid"].ToString()),
                        StartTime = DateTime.Parse(data["scheduledstart"].ToString()),
                        StartTimeDisplayName = data["scheduledstart@OData.Community.Display.V1.FormattedValue"],
                        EndTime = DateTime.Parse(data["scheduledend"].ToString()),
                        EndTimeDisplayName = data["scheduledend@OData.Community.Display.V1.FormattedValue"],
                        OwnerId = Guid.Parse(data["_ownerid_value"].ToString()),
                        Owner = new SystemUser()
                        {
                            Id = Guid.Parse(data["_ownerid_value"].ToString()),
                            Fullname = data["_ownerid_value@OData.Community.Display.V1.FormattedValue"]
                        }
                    };
                    appointments.Add(appointment);
                }

                var endDate = appointments.Max(r => r.EndTime);
                var startDate = DateTime.Today;


                foreach (var date in appointmentDates)
                {
                    Console.WriteLine(date);
                    for (var min = 0; min < 1440; min += 30)
                    {
                        var apptCount = appointments.Count(x => x.StartTime.Date == date.Date
                                    && x.StartTime.TimeOfDay.TotalMinutes <= min
                                    && x.EndTime.TimeOfDay.TotalMinutes >= (min + 30));
                        if (apptCount >= numberOfInspectors)
                        {
                            bookedAppointments.Add(new Appointment()
                            {
                                Subject = "Booked",
                                StartTime = new DateTime(date.Year, date.Month, date.Day, (int)(min / 60), ((min / 30) % 2 * 30), 0),
                                EndTime = new DateTime(date.Year, date.Month, date.Day, (int)((min + 30) / 60), (((min + 30) / 30) % 2 * 30), 0)
                            });
                        }
                    }
                }

                return bookedAppointments;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<Appointment> QueryAppointmentsByInspector(HttpClient httpClient, Guid inspectorId)
        {

            var appointments = new List<Appointment>();

            try
            {

                var fetchXML = string.Empty;
                fetchXML += "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>";
                fetchXML += "  <entity name='appointment'>";
                fetchXML += "    <attribute name='statecode' />";
                fetchXML += "    <attribute name='scheduledstart' />";
                fetchXML += "    <attribute name='scheduledend' />";
                fetchXML += "    <attribute name='ownerid' />";
                fetchXML += "    <attribute name='activityid' />";
                fetchXML += "    <filter type='and'>";
                fetchXML += "      <condition attribute='statecode' operator='in'>";
                fetchXML += "        <value>0</value>";
                fetchXML += "        <value>3</value>";
                fetchXML += "      </condition>";
                fetchXML += "      <condition attribute='blu_approvalstatus' operator='eq' value='" + (int)ApprovalStatus.New + "' />";
                fetchXML += "      <condition attribute='ownerid' operator='eq' value='" + inspectorId + "' />";
                fetchXML += "      <condition attribute='scheduledstart' operator='on-or-after' value='" + DateTime.Today.ToShortDateString() + "' />";
                fetchXML += "    </filter>";
                fetchXML += "  </entity>";
                fetchXML += "</fetch>";

                var encodedQuery = SharedService.UrlEncode(fetchXML);
                var odataQuery = webApiQueryUrl + "appointments?fetchXml=" +
                                 encodedQuery;

                var retrieveResponse = httpClient.GetAsync(odataQuery);
                var jRetrieveResponse = JObject.Parse(retrieveResponse.Result.Content.ReadAsStringAsync().Result);
                dynamic systemUserObject = JsonConvert.DeserializeObject(jRetrieveResponse.ToString());
                foreach (var data in systemUserObject.value)
                {

                    var appointment = new Appointment()
                    {
                        Id = data["activityid"] == null
                            ? Guid.Empty
                            : Guid.Parse(data["activityid"].ToString()),
                        StartTime = DateTime.Parse(data["scheduledstart"].ToString()),
                        StartTimeDisplayName = data["scheduledstart@OData.Community.Display.V1.FormattedValue"],
                        EndTime = DateTime.Parse(data["scheduledend"].ToString()),
                        EndTimeDisplayName = data["scheduledend@OData.Community.Display.V1.FormattedValue"],
                        OwnerId = Guid.Parse(data["_ownerid_value"].ToString()),
                        Owner = new SystemUser()
                        {
                            Id = Guid.Parse(data["_ownerid_value"].ToString()),
                            Fullname = data["_ownerid_value@OData.Community.Display.V1.FormattedValue"]
                        }
                    };
                    appointments.Add(appointment);

                }

                return appointments;

            }
            catch (Exception ex)
            {
                LogService.LogMessage(httpClient, new Log()
                {
                    Level = (int)LogType.Error,
                    Name = ex.Message,
                    FunctionName = ClassName + " | " + MethodBase.GetCurrentMethod().Name,
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                });
                return null;
            }

        }

    }
}
