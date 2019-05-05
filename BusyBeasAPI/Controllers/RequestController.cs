using BusyBeasAPI.Models;
using BusyBeasDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using System.Web.Http.Cors;

namespace BusyBeasAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("Request")]
    public class RequestController : ApiController
    {
        [Route("GetServices")]
        [HttpGet]
        public List<ServiceModel> GetServices()
        {
            using (var context = new BusyBeasEntities())
            {
                try
                {
                    var services = context.services.ToList();
                    var result = new List<ServiceModel>();

                    foreach (var s in services)
                    {
                        result.Add(new ServiceModel()
                        {
                            id = s.id,
                            serviceName = s.serviceName,
                            serviceDescription = s.serviceDescription,
                            pricePerHour = s.pricePerHour
                        });

                    }
                    return result;
                }
                catch (NullReferenceException e)
                {
                    HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No services found.");
                    throw new HttpResponseException(response);
                }
            }
        }

        [Route("CreateRequest")]
        [HttpGet]
        public void CreateRequest(int uId, DateTime date, int sId, string email)
        {
            using (var context = new BusyBeasEntities())
            {
                try
                {
                    context.serviceRequests.Add(new serviceRequest()
                    {
                        userId = uId,
                        scheduledDate = date,
                        serviceId = sId,
                        fulfilled = false,
                        hoursTaken = 0
                    });

                    context.SaveChanges();
                    sendRequestEmail(uId, sId, date, email);
                }
                catch (NullReferenceException e)
                {
                    HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot add request.");
                    throw new HttpResponseException(response);
                }
            }
        }

        [Route("GetRequestedServices")]
        [HttpGet]
        public List<RequestedServicesModel> GetRequestedServices(int userId)
        {
            using (var context = new BusyBeasEntities())
            {
                try
                {
                    var services = from sr in context.serviceRequests
                                   join s in context.services on sr.serviceId equals s.id
                                   where sr.userId == userId
                                   select new
                                   {
                                       sr.id,
                                       s.serviceName,
                                       sr.scheduledDate,
                                       sr.fulfilled,
                                       sr.hoursTaken,
                                       s.pricePerHour
                                   };

                    var result = new List<RequestedServicesModel>();

                    foreach (var s in services)
                    {
                        result.Add(new RequestedServicesModel()
                        {
                            id = s.id,
                            serviceName = s.serviceName,
                            date = s.scheduledDate,
                            fulfilled = s.fulfilled,
                            hoursTaken = s.hoursTaken,
                            pricePerHour = s.pricePerHour
                        });

                    }
                    return result;
                }
                catch (NullReferenceException e)
                {
                    HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No service requests found.");
                    throw new HttpResponseException(response);
                }
            }
        }
        public void sendRequestEmail(int userId, int serviceId, DateTime date, string email)
        {
            using (var context = new BusyBeasEntities())
            {
                var user = context.users.Where(x => x.id == userId).FirstOrDefault();
                var address = context.addresses.Where(x => x.userId == userId).FirstOrDefault();
                var service = context.services.Where(x => x.id == serviceId).FirstOrDefault();

                MailMessage mail = new MailMessage();
                mail.To.Add(email);
                mail.To.Add("hrfloyd1s@gmail.com");
                mail.To.Add("busybeascleaningservice@gmail.com");
                mail.From = new MailAddress("busybeascleaningservice@gmail.com");
                mail.Subject = "Service Request for service: " + service.serviceName + " on date: "  + date.ToString("dd/MM/yyyy");


                mail.Body = "Hi " + user.firstname + "!<br><br>" +
                    "This is an email confirmation for services requested for date: " + date + ".<br><br>" +
                    "The service requested was " + service.serviceName + ". " +
                    "This service typically takes 30 to 45 minutes per room, so please plan according to your needs.<br><br>" +
                    "The address we have on file is:<br>" + address.street + "<br>" + address.city + " " + address.state + ", " + address.zip + "<br><br>" +
                    "If you have any questions or concerns, please call Hannah at 573-837-5668 or " +
                    "reply to this email.<br><br>" +
                    "Thank You,<br> Hannah";

                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com"; //Or Your SMTP Server Address
                smtp.Credentials = new System.Net.NetworkCredential
                     ("busybeascleaningservice@gmail.com", "BusyBeas1234");
                //Or your Smtp Email ID and Password
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
        }
    }
}