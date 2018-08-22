using Microsoft.Crm.Sdk.Messages;
using Microsoft.Pfe.Xrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationServiceProxyIssue
{
    internal class Program
    {
        private static Uri serviceUri = new Uri("http://***/XRMServices/2011/Organization.svc");

        private static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 100;

            //https://docs.microsoft.com/en-us/dynamics365/customer-engagement/developer/best-practices-sdk#improve-service-channel-allocation-performance

            PlainSdk();
            Pfe();

            Console.ReadLine();
        }

        private static void PlainSdk()
        {
            try
            {
                Console.WriteLine("Testing plain sdk");
                var defaultCredentials = new AuthenticationCredentials()
                {
                    ClientCredentials = new ClientCredentials()
                    {
                        Windows = { ClientCredential = CredentialCache.DefaultNetworkCredentials }
                    }
                };
                IServiceManagement<IOrganizationService> orgServiceManagement =
                    ServiceConfigurationFactory.CreateManagement<IOrganizationService>(serviceUri);

                // go crazy on creating new proxies to simulate, for example, a high traffic web api.
                Parallel.ForEach(Enumerable.Range(0, 100000), (index) =>
                {
                    var proxy = new OrganizationServiceProxy(orgServiceManagement, defaultCredentials.ClientCredentials);

                    // From the link above:
                    // If you enable early-bound types on OrganizationServiceProxy through one of the EnableProxyTypes() methods,
                    // you must do the same on all service proxies that are created from the cached IServiceManagement < TService > object.
                    proxy.EnableProxyTypes();

                    proxy.Execute(new WhoAmIRequest());
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Pfe()
        {
            try
            {
                Console.WriteLine("Testing plain pfe");
                var manager = new OrganizationServiceManager(serviceUri);

                Parallel.ForEach(Enumerable.Range(0, 100000), (index) =>
                {
                    var proxy = manager.GetProxy();
                    proxy.EnableProxyTypes();
                    proxy.Execute(new WhoAmIRequest());
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}