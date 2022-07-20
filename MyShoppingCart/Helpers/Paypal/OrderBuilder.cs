using System;
using System.Collections.Generic;
using MyshoppingCart.Helpers.ContainerPackaging;
using ContainerEntities = MyshoppingCart.Helpers.ContainerPackaging.Entities;
using MyShoppingCart.Data.ViewModels;
using MyShoppingCart.Helpers.ContainerPackaging.Algorithms;
using MyShoppingCart.Helpers.Paypal.Values;
using MyShoppingCart.Models;
using PaypalSdk = PayPalCheckoutSdk.Orders;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using MyShoppingCart.Models.USPS;
using System.Xml;

namespace MyShoppingCart.Helpers.Paypal
{
    public static class OrderBuilder
    {
        /// <summary>
        /// Use classes from the PayPalCheckoutSdk to build an OrderRequest
        /// </summary>
        /// <returns></returns>
        public static async Task<PaypalSdk.OrderRequest> Build(ShoppingCartVM basket)
        {

            //var basket = unitOfWork.Baskets.SingleOrDefault(x => x.Id == x.Id);

            if (basket == null)
                return null;

            //https://developer.paypal.com/docs/api/reference/locale-codes/#


            PaypalSdk.OrderRequest orderRequest = new PaypalSdk.OrderRequest()
            {
                CheckoutPaymentIntent = CheckoutPaymentIntent.CAPTURE,
                ApplicationContext = new PaypalSdk.ApplicationContext
                {
                    BrandName = "myshoppingcart.biz",
                    LandingPage = Values.LandingPage.LOGIN,
                    UserAction = Values.UserAction.PAY_NOW,
                    ShippingPreference = Values.ShippingPreference.GET_FROM_FILE,
                    Locale = "en-US"
                },
                PurchaseUnits = new List<PaypalSdk.PurchaseUnitRequest>
                {
                    new PaypalSdk.PurchaseUnitRequest
                    {
                        //ReferenceId = "Delaneys.space", // [required] The merchant ID for the purchase unit.
                        Description = "Software published by Delaneys.space",
                        SoftDescriptor = "Delaneys.space",
                        AmountWithBreakdown = new PaypalSdk.AmountWithBreakdown
                        {
                            CurrencyCode = Values.CurrencyCode.USD,
                            Value = "16.99", // basket.ShoppingCartTotal.ToString(),
                            AmountBreakdown = new PaypalSdk.AmountBreakdown
                            {
                                ItemTotal = new PaypalSdk.Money
                                {
                                    CurrencyCode = CurrencyCode.USD,
                                    Value = "12.99"
                                },
                                TaxTotal = new PaypalSdk.Money
                                {
                                    CurrencyCode= CurrencyCode.USD,
                                    Value="1.00"
                                },
                                Shipping = new PaypalSdk.Money
                                {
                                    CurrencyCode= CurrencyCode.USD,
                                    Value = await CalculateShipping(basket) //"3.00"
                                }

                                //,
                                //Discount = new Money
                                //{
                                //    CurrencyCode = CurrencyCode.USD,
                                //    Value = basket.Discount.ToString()
                                //}
                            }
                        },
                        Items = new List<PaypalSdk.Item>()
                    }
                }
            };

            foreach (var product in basket.ShoppingCart.ShoppingCartItems)
            {
                orderRequest.PurchaseUnits[0]
                            .Items
                            .Add(new PaypalSdk.Item
                            {
                                Name = product.Product.Name,
                                Description = product.Product.Description.Trim().Length > 127 ? product.Product.Description[..126] : product.Product.Description.Trim(),
                                UnitAmount = new PaypalSdk.Money
                                {
                                    CurrencyCode = CurrencyCode.USD,
                                    Value = product.Product.Price.ToString()
                                },
                                Quantity = product.Amount.ToString(),
                                Category = Values.Item.Category.DIGITAL_GOODS
                            });
            }


            return orderRequest;
        }

        private async static Task<string> CalculateShipping(ShoppingCartVM basket)
        {
            //https://github.com/davidmchapman/3DContainerPacking
            //List<ContainerPackingResult> result = PackingService.Pack(containers, itemsToPack, algorithms);

            HttpClient client = new HttpClient();

            var stringTask = client.GetStringAsync("https://secure.shippingapis.com/ShippingAPI.dll?API=RateV4&XML=<RateV4Request USERID=\"928UNKNO7333\"><Revision>2</Revision><Package ID=\"0\"><Service>FIRST CLASS</Service><FirstClassMailType>PACKAGE SERVICE RETAIL</FirstClassMailType><ZipOrigination>22201</ZipOrigination><ZipDestination>26301</ZipDestination><Pounds>0</Pounds><Ounces>3</Ounces><Container></Container><Width>4.5</Width><Length>6.3</Length><Height>15</Height><Girth></Girth><Machinable>TRUE</Machinable></Package><Package ID=\"1\"><Service>FIRST CLASS</Service><FirstClassMailType>PACKAGE SERVICE RETAIL</FirstClassMailType><ZipOrigination>22201</ZipOrigination><ZipDestination>26301</ZipDestination><Pounds>0</Pounds><Ounces>3</Ounces><Container></Container><Width></Width><Length></Length><Height></Height><Girth></Girth><Machinable>TRUE</Machinable></Package><Package ID=\"2\"><Service>FIRST CLASS</Service><FirstClassMailType>PACKAGE SERVICE RETAIL</FirstClassMailType><ZipOrigination>22201</ZipOrigination><ZipDestination>26301</ZipDestination><Pounds>0</Pounds><Ounces>3</Ounces><Container></Container><Width></Width><Length></Length><Height></Height><Girth></Girth><Machinable>TRUE</Machinable></Package></RateV4Request>");

            var msg = await stringTask;

            //XmlDocument xmldoc = new XmlDocument();
            //xmldoc.LoadXml(msg);
            //XmlNodeList MailServicenodeList = xmldoc.GetElementsByTagName("MailService")[0].InnerText;
            //XmlNodeList RatenodeList = xmldoc.GetElementsByTagName("Rate");
            //string Short_Fall = string.Empty;
            //foreach (XmlNode node in nodeList)
            //{
            //    Short_Fall = node.InnerText;
            //}

            //XmlSerializer serializer = new XmlSerializer(typeof(Postage));

            //StringReader reader = new StringReader(msg);

            //PostageCollection postages = new PostageCollection();

            //postages = serializer.Deserialize(reader) as PostageCollection;
            //reader.Close();

            List<int> algorithms = GetPackingAlgorithms();
            List<ContainerEntities.Item> itemsToPack = GetShoppingCartItems(basket);
            List<ContainerEntities.Container> containers = GetShippingContainers(basket);

            List<ContainerEntities.ContainerPackingResult> result = PackingService.Pack(containers, itemsToPack, algorithms);

            List<ContainerEntities.ContainerPackingResult> packingResults_NoUnpackedItems_IsCompletePacked = 
                result.FindAll(x => x.AlgorithmPackingResults.Exists(y => y.IsCompletePack == true && y.UnpackedItems.Count == 0));

            //if(result.Exists(x => x.AlgorithmPackingResults.FindAll(x  => x.IsCompletePack == true)))

            //if(basket.ShoppingCart.ShoppingCartItems.Count == 0)    
            return "3.00";
        }

        private static List<int> GetPackingAlgorithms()
        {
            return new List<int> { (int)AlgorithmType.EB_AFIT };
        }

        private static List<ContainerEntities.Container> GetShippingContainers(ShoppingCartVM basket)
        {
            List<ContainerEntities.Container> containers = new List<ContainerEntities.Container>();

            foreach (ShippingMethod shippingMethod in basket.ShippingMethods)
            {
                ContainerEntities.Container container = new
                    (
                        shippingMethod.container.Id,
                        shippingMethod.container.Name,
                        shippingMethod.container.LengthInch,
                        shippingMethod.container.WidthInch,
                        shippingMethod.container.HeightInch
                    );
                containers.Add(container);
            }
            return containers;
        }

        private static List<ContainerEntities.Item> GetShoppingCartItems(ShoppingCartVM basket)
        {
            List<ContainerEntities.Item> itemsToPack = new List<ContainerEntities.Item>();

            foreach (ShoppingCartItem shoppingCartItem in basket.ShoppingCart.ShoppingCartItems)
            {
                ContainerEntities.Item item = new
                    (
                        shoppingCartItem.Product.Id,
                        shoppingCartItem.Product.length,
                        shoppingCartItem.Product.width,
                        shoppingCartItem.Product.height,
                        shoppingCartItem.Product.weight,
                        shoppingCartItem.Amount
                    );
                itemsToPack.Add(item);
            }
            return itemsToPack;
        }
    }
}
