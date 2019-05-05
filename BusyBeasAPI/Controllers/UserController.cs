using BusyBeasAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using BusyBeasDataAccess;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net;

namespace BusyBeasAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("User")]
    public class UserController : ApiController
    {
        [Route("GetUserForLogin")]
        [HttpGet]
        public UserModel GetUserForLogin(string username, string password)
        {
            using (var context = new BusyBeasEntities())
            {
                try
                {
                    var un = hash(username);
                    var up = hash(password);
                    var user = context.users.Where(x => x.email.ToLower() == un.ToLower()).FirstOrDefault();

                    var success = unHashDB(user.pass, up);

                    if (!success)
                    {
                        HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User email and password combination does not match our records.");
                        throw new HttpResponseException(response);
                    }
                    else if (success)
                    {

                        var result = new UserModel()
                        {
                            id = user.id,
                            email = user.email,
                            firstName = user.firstname,
                            lastName = user.lastname,
                            isAdmin = user.isAdmin
                        };
                        return result;
                    };
                }
                catch (NullReferenceException e)
                {
                    HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Email does not match account in our records.");
                    throw new HttpResponseException(response);
                }
                return null;
            }
        }

        [Route("CreateNewUser")]
        [HttpGet]
        public void CreateNewUser(string username, string password, string firstname, string lastname)
        {
            using (var context = new BusyBeasEntities())
            {
                try
                {
                    var un = hash(username);
                    var up = hash(password);
                    var f = hash(firstname);
                    var l = hash(lastname);
                    up = hashDB(up);
                    context.users.Add(new user()
                    {
                        email = un,
                        pass = up,
                        firstname = f,
                        lastname = l,
                        isAdmin = false,
                        CREATE_TS = DateTime.Now,
                        UPDATE_TS = DateTime.Now
                    });
                    context.SaveChanges();
                }
                catch (NullReferenceException e)
                {

                }
            }
        }

        [Route("GetClients")]
        [HttpGet]
        public List<UserModel> GetClients()
        {
            using (var context = new BusyBeasEntities())
            {
                try
                {
                    var users = context.users.Where(x => x.isAdmin == false).ToList();
                    var result = new List<UserModel>();

                    foreach (var u in users)
                    {
                        result.Add(new UserModel()
                        {
                            id = u.id,
                            email = u.email,
                            firstName = u.firstname,
                            lastName = u.lastname,
                            isAdmin = u.isAdmin
                        });

                    }
                    return result;
                }
                catch (NullReferenceException e)
                {
                    HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Clients found.");
                    throw new HttpResponseException(response);
                }
            }
        }

        [Route("UpdateAddress")]
        [HttpGet]
        public void UpdateAddress(int userid, string lineOne, string lineTwo, string city, string state, string zip)
        {
            using (var context = new BusyBeasEntities())
            {
                try
                {
                    var address = context.addresses.Where(x => x.userId == userid && x.isDelete == false).FirstOrDefault();

                    if (address == null)
                    {
                        context.addresses.Add(new address()
                        {
                            userId = userid,
                            street = lineOne,
                            streetTwo = lineTwo,
                            city = city,
                            state = state,
                            zip = zip,
                            isDelete = false,
                            CREATE_TS = DateTime.Now,
                            UPDATE_TS = DateTime.Now
                        });
                    }
                    else
                    {
                        address.userId = userid;
                        address.street = lineOne;
                        address.streetTwo = lineTwo;
                        address.city = city;
                        address.state = state;
                        address.zip = zip;
                        address.UPDATE_TS = DateTime.Now;
                    }

                    context.SaveChanges();

                }
                catch (NullReferenceException e)
                {
                    HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Add address failed.");
                    throw new HttpResponseException(response);
                }
            }
        }

        [Route("GetAddress")]
        [HttpGet]
        public AddressModel GetAddress(int userid)
        {
            using (var context = new BusyBeasEntities())
            {
                try
                {
                    var address = context.addresses.Where(x => x.userId == userid && x.isDelete == false).FirstOrDefault();

                    return new AddressModel()
                    {
                        id = address.id,
                        userId = address.userId,
                        streetOne = address.street,
                        streetTwo = address.streetTwo,
                        city = address.city,
                        state = address.state,
                        zip = address.zip
                    };
                }
                catch (NullReferenceException e)
                {
                    HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot find address.");
                    throw new HttpResponseException(response);
                }
            }
        }

        private string hash(string word)
        {
            var alphabet = "abcdefghijklmnopqrstuvwxyz";
            var fullAlphabet = alphabet + alphabet + alphabet;
            var key = 5;

            key = (key % alphabet.Length);
            key = key * -1;
            word = word.ToLower();
            var cipherFinish = "";

            for (var i = 0; i < word.Length; i++)
            {
                var letter = word[i];

                var index = alphabet.IndexOf(letter);
                if (index == -1)
                {
                    cipherFinish += letter;
                }
                else
                {
                    index = ((index + key) + alphabet.Length);
                    var nextLetter = fullAlphabet[index];
                    cipherFinish += nextLetter;
                }
            }

            return cipherFinish;
        }

        private string hashDB(string word)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdsf2 = new Rfc2898DeriveBytes(word, salt, 10000);

            byte[] hash = pbkdsf2.GetBytes(20);

            byte[] hashBytes = new byte[36];

            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            string savedPasswordHash = Convert.ToBase64String(hashBytes);

            return savedPasswordHash;

        }

        private bool unHashDB(string saved, string pass)
        {
            byte[] hashBytes = Convert.FromBase64String(saved);

            byte[] salt = new byte[16];

            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdsf2 = new Rfc2898DeriveBytes(pass, salt, 10000);

            byte[] hash = pbkdsf2.GetBytes(20);

            int ok = 1;

            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    ok = 0;
                }
            }

            if (ok == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}