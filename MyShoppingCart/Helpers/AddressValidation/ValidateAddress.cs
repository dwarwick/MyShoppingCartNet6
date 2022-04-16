using MyShoppingCart.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyShoppingCart.Helpers.AddressValidation
{
    public static class ValidateAddress
    {
        public static async Task<AddressVerificationResponse> VerifyAddressAsync(ApplicationUser applicationUser)
        {
            AddressVerificationResponse addressVerificationResponse = new AddressVerificationResponse();

            string payload = "";
            try
            {
                payload = $"{{\"TransmissionReference\": \"Test\",\"Actions\": \"Check\",\"CustomerID\":\"hcDwJZ2Bxpz5ogAEiqpBta**nSAcwXpxhQ0PC2lXxuDAZ-**\",\"Records\":[{{\"AddressLine1\":\"{applicationUser.Address1}\",\"AddressLine2\":\"{applicationUser.Address2}\",\"City\":\"{applicationUser.City}\",\"PostalCode\":\"{applicationUser.Zip}\",\"State\":\"{applicationUser.State}\"}}]}}";
            }
            catch (Exception ex)
            {

                string error = ex.Message;
            }

            HttpResponseMessage response;
            using (var client = new HttpClient())
            {
                HttpContent c = new StringContent(payload, Encoding.UTF8, "application/json");
                response = await client.PostAsync(@"https://personator.melissadata.net/v3/WEB/ContactVerify/doContactVerify?t=", c);
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    RootObject responeObj = JsonConvert.DeserializeObject<RootObject>(result);

                    addressVerificationResponse = AnalyzeResponse(responeObj);
                    addressVerificationResponse.rootObject = responeObj;
                }
            }

            return addressVerificationResponse;
        }

        private static AddressVerificationResponse AnalyzeResponse(RootObject rootObject)
        {
            AddressVerificationResponse addressVerificationResponse = new AddressVerificationResponse();

            string responseCode = rootObject.Data[0].Results;
            string[] responseCodes = responseCode.Split(',');

            foreach (string code in responseCodes)
            {
                switch (code)
                {
                    case "AS01":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = false;
                        addressVerificationResponse.ErrorText += "";
                        break;

                    case "AS02":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The street address was verified but the suite/apartment number is missing or invalid.";
                        break;

                    case "AE01":
                        addressVerificationResponse.AllowAsIs = false;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The address could not be verified at least up to the postal code level.";
                        break;

                    case "AE02":
                        addressVerificationResponse.AllowAsIs = false;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "Could not match the input street to a unique street name. Either no matches or too many matches found.";
                        break;

                    case "AE03":
                        addressVerificationResponse.AllowAsIs = false;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The combination of directionals (N, E, SW, etc) and the suffix (AVE, ST, BLVD) is not correct and produced multiple possible matches.";
                        break;

                    case "AE04":
                        addressVerificationResponse.AllowAsIs = false;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "A physical plot exists but is not a deliverable addresses. One example might be a railroad track or river running alongside this street, as they would prevent construction of homes in that location.";
                        break;

                    case "AE05":
                        addressVerificationResponse.AllowAsIs = false;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The address was matched to multiple records. There is not enough information available in the address to break the tie between multiple records.";
                        break;

                    case "AE06":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = false;
                        addressVerificationResponse.ErrorText += "This address currently cannot be verified but was identified by the Early Warning System (EWS) as belonging to a upcoming area and will likely be included in a future update.";
                        break;

                    case "AE07":
                        addressVerificationResponse.AllowAsIs = false;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "Minimum requirements for the address to be verified is not met. A country input is required for global products. For US/CA, a postal code or city/state are required. Requirements for other countries may be different.";
                        break;

                    case "AE08":
                        addressVerificationResponse.AllowAsIs = false;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "An address element after the house number, in most cases the sub-premise, was not valid.";
                        break;

                    case "AE09":
                        addressVerificationResponse.AllowAsIs = false;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "An address element after the house number, in most cases the sub-premise, was missing.";
                        break;

                    case "AE10":
                        addressVerificationResponse.AllowAsIs = false;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The premise (house or building) number for the address is not valid.";
                        break;

                    case "AE11":
                        addressVerificationResponse.AllowAsIs = false;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The premise (house or building) number for the address is missing.";
                        break;

                    case "AE12":
                        addressVerificationResponse.AllowAsIs = false;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The PO (Post Office Box), RR (Rural Route), or HC (Highway Contract) Box number is invalid.";
                        break;

                    case "AE13":
                        addressVerificationResponse.AllowAsIs = false;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The PO (Post Office Box), RR (Rural Route), or HC (Highway Contract) Box number is missing.";
                        break;

                    case "AE14":
                        addressVerificationResponse.AllowAsIs = false;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The address is a Commercial Mail Receiving Agency (CMRA) and the Private Mail Box (PMB or #) number is missing.";
                        break;

                    case "AC01":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The postal code was changed or added.";
                        break;

                    case "AC02":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The administrative area (state, province) was added or changed.";
                        break;

                    case "AC03":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The locality (city, municipality) name was added or changed.";
                        break;

                    case "AC04":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The address was found to be an alternate record and changed to the base (preferred) version.";
                        break;

                    case "AC05":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "An alias is a common abbreviation for a long street name, such as “MLK Blvd” for “Martin Luther King Blvd.” This change code indicates that the full street name (preferred) has been substituted for the alias.";
                        break;

                    case "AC06":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "Address1 was swapped with Address2 because Address1 could not be verified and Address2 could be verified.";
                        break;

                    case "AC08":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "A non-empty plus4 was changed.";
                        break;

                    case "AC09":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The dependent locality (urbanization) was changed.";
                        break;

                    case "AC10":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The thoroughfare (street) name was added or changed due to a spelling correction.";
                        break;

                    case "AC11":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The thoroughfare (street) leading or trailing type was added or changed, such as from \"St\" to \"Rd.\"";
                        break;

                    case "AC12":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The thoroughfare (street) pre-directional or post-directional was added or changed, such as from \"N\" to \"NW.\"";
                        break;

                    case "AC13":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The sub premise (suite) type was added or changed, such as from \"STE\" to \"APT.\"";
                        break;

                    case "AC14":
                        addressVerificationResponse.AllowAsIs = true;
                        addressVerificationResponse.IsError = true;
                        addressVerificationResponse.ErrorText += "The sub premise (suite) unit number was added or changed.";
                        break;
                }
            }
            return addressVerificationResponse;
        }
    }
}

//string json = string.Format("{" +
//     "'TransmissionReference':" + "'Test'," +
//     "'Actions':" + "'Check'," +
//     "'Columns':" + "''," +
//     "'CustomerID':" + "'hcDwJZ2Bxpz5ogAEiqpBta**nSAcwXpxhQ0PC2lXxuDAZ-**'," +
//     "'Options':" + "''," +
//     "'Records':[" +
//     "{" +
//     "'AddressLine1':" + "'{0}'," +
//     "'AddressLine2':" + "'{1}'," +
//     "'City':" + "'{2}'," +
//     "'PostalCode':" + "'{3}'," +
//     "'State':" + "'{4}'," +
//     "}" +
//     "]" +
//     "}", applicationUser.Address1, applicationUser.Address2, applicationUser.City, applicationUser.Zip, applicationUser.State
//     );

