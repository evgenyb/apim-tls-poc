using System;
using Pulumi;
using Pulumi.Azure.ApiManagement;
using Pulumi.Azure.ApiManagement.Inputs;
using Pulumi.Azure.Core;
using Pulumi.Azure.Network;
using Pulumi.Azure.Authorization;
using Pulumi.Azure.EventHub;
using Pulumi.Azure.Network.Inputs;

class MyStack : Stack
{
    public MyStack()
    {
        var config = new Config();
        var environment = Deployment.Instance.StackName;
        
        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("rg", new ResourceGroupArgs
        {
            Name = NamingConvention.GetResourceGroupName(environment)
        });

        var vnet = new VirtualNetwork("vnet", new VirtualNetworkArgs
        {
            Name = NamingConvention.GetVNetName(environment),
            ResourceGroupName = resourceGroup.Name,
            AddressSpaces =
            {
                config.Require("vnet.addressSpaces")
            }
        });
     
        // Create a Subnet for the cluster
        var apimSubnet = new Subnet("apim-net", new SubnetArgs
        {
            Name = "apim-net",
            ResourceGroupName = resourceGroup.Name,
            VirtualNetworkName = vnet.Name,
            AddressPrefixes =
            {
                config.Require("vnet.subnets.apim.addressPrefixes")
            },
        });
        
        // Create a Subnet for the afw
        var firewallSubnet = new Subnet("afw-net", new SubnetArgs
        {
            Name = "AzureFirewallSubnet",
            ResourceGroupName = resourceGroup.Name,
            VirtualNetworkName = vnet.Name,
            AddressPrefixes =
            {
                config.Require("vnet.subnets.afw.addressPrefixes")
            },
        });
        
        var agwSubnet = new Subnet("agw-net", new SubnetArgs
        {
            Name = "agw-net",
            ResourceGroupName = resourceGroup.Name,
            VirtualNetworkName = vnet.Name,
            AddressPrefixes =
            {
                config.Require("vnet.subnets.agw.addressPrefixes")
            },
        });        
        
        var privateEndpointSubnet = new Subnet("functions-net", new SubnetArgs
        {
            Name = "functions-net",
            ResourceGroupName = resourceGroup.Name,
            VirtualNetworkName = vnet.Name,
            AddressPrefixes =
            {
                config.Require("vnet.subnets.functions.addressPrefixes")
            },
        });        

        var eventHubNamespace = new EventHubNamespace("ehn", new EventHubNamespaceArgs
        {
            Name = "iac-apim-logging-ns",
            Location = resourceGroup.Location,
            ResourceGroupName = resourceGroup.Name,
            Sku = "Standard",
            Capacity = 1
        });
        
        var eventHub = new EventHub("eh", new EventHubArgs
        {
            Name = "apim-logging", 
            NamespaceName = eventHubNamespace.Name,
            ResourceGroupName = resourceGroup.Name,
            PartitionCount = 2,
            MessageRetention = 1,
        });
        
        var apimExternal = new Service("apim-external", new ServiceArgs
        {
            Name = "iac-dev-ext-apim",
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            PublisherName = "IaC",
            PublisherEmail = "evgeny.borzenin@gmail.com",
            SkuName = "Developer_1",
            Identity = new ServiceIdentityArgs
            {
                Type = "SystemAssigned"
            },
            VirtualNetworkType = "External",
            VirtualNetworkConfiguration = new ServiceVirtualNetworkConfigurationArgs
            {
                SubnetId = apimSubnet.Id
            },
            HostnameConfiguration = new ServiceHostnameConfigurationArgs
            {
                Proxies = new []
                {
                    new ServiceHostnameConfigurationProxyArgs
                    {
                        HostName = "iac-dev-ext-apim.azure-api.net",
                        DefaultSslBinding = false,
                        NegotiateClientCertificate = false
                    },
                    new ServiceHostnameConfigurationProxyArgs
                    {
                        HostName = "api.iac-labs.com",
                        KeyVaultId = config.Require("certificate.keyvaultid"),
                        DefaultSslBinding = true,
                        NegotiateClientCertificate = false
                    },
                    new ServiceHostnameConfigurationProxyArgs
                    {
                        HostName = "api29cc67d2.iac-labs.com",
                        KeyVaultId = config.Require("certificate.keyvaultid"),
                        DefaultSslBinding = true,
                        NegotiateClientCertificate = false
                    }
                }
            }
        });

      
        var apimInternal = new Service("apim-int", new ServiceArgs
        {
            Name = NamingConvention.GetApimName(environment),
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            PublisherName = "IaC",
            PublisherEmail = "evgeny.borzenin@gmail.com",
            SkuName = "Developer_1",
            Identity = new ServiceIdentityArgs
            {
                Type = "SystemAssigned"
            },
            VirtualNetworkType = "Internal",
            VirtualNetworkConfiguration = new ServiceVirtualNetworkConfigurationArgs
            {
                SubnetId = apimSubnet.Id
            },
            HostnameConfiguration = new ServiceHostnameConfigurationArgs
            {
                Proxies = new []
                {
                    new ServiceHostnameConfigurationProxyArgs
                    {
                        HostName = "iac-dev-apim.azure-api.net",
                        DefaultSslBinding = false,
                        NegotiateClientCertificate = false
                    },
                    new ServiceHostnameConfigurationProxyArgs
                    {
                        HostName = "api.iac-labs.com",
                        KeyVaultId = config.Require("certificate.keyvaultid"),
                        DefaultSslBinding = true,
                        NegotiateClientCertificate = false
                    },
                    new ServiceHostnameConfigurationProxyArgs
                    {
                        HostName = "api29cc67d2.iac-labs.com",
                        KeyVaultId = config.Require("certificate.keyvaultid"),
                        DefaultSslBinding = true,
                        NegotiateClientCertificate = false
                    },
                    new ServiceHostnameConfigurationProxyArgs
                    {
                        HostName = "api-internal.iac-labs.com",
                        KeyVaultId = config.Require("certificate.keyvaultid"),
                        DefaultSslBinding = true,
                        NegotiateClientCertificate = false
                    }
                }
            }
        });

        var apim1 = Output.Create(GetService.InvokeAsync(new GetServiceArgs
        {
            Name = "iac-dev-apim1",
            ResourceGroupName = "iac-dev-rg",
        }));

        if (config.RequireBoolean("firstTimeAPIM"))
        {
            var apimInternal1 = new Service("apim-int1", new ServiceArgs
            {
                Name = "iac-dev-apim1",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                PublisherName = "IaC",
                PublisherEmail = "evgeny.borzenin@gmail.com",
                SkuName = "Developer_1",
                Identity = new ServiceIdentityArgs
                {
                    Type = "SystemAssigned"
                },
                VirtualNetworkType = "Internal",
                VirtualNetworkConfiguration = new ServiceVirtualNetworkConfigurationArgs
                {
                    SubnetId = apimSubnet.Id
                }
            });
        }
        else
        {
            var apimInternal1 = new Service("apim-int1", new ServiceArgs
            {
                Name = "iac-dev-apim1",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                PublisherName = "IaC",
                PublisherEmail = "evgeny.borzenin@gmail.com",
                SkuName = "Developer_1",
                Identity = new ServiceIdentityArgs
                {
                    Type = "SystemAssigned"
                },
                VirtualNetworkType = "Internal",
                VirtualNetworkConfiguration = new ServiceVirtualNetworkConfigurationArgs
                {
                    SubnetId = apimSubnet.Id
                },
                HostnameConfiguration = new ServiceHostnameConfigurationArgs
                {
                    Proxies = new []
                    {
                        new ServiceHostnameConfigurationProxyArgs
                        {
                            HostName = "iac-dev-apim1.azure-api.net",
                            DefaultSslBinding = false,
                            NegotiateClientCertificate = false
                        },
                        new ServiceHostnameConfigurationProxyArgs
                        {
                            HostName = "api.iac-labs.com",
                            KeyVaultId = config.Require("certificate.keyvaultid"),
                            DefaultSslBinding = true,
                            NegotiateClientCertificate = false
                        },
                    }
                }
            });
        }

        var ehLogger = new Logger("ehLogger", new LoggerArgs
        {
            Name = "ehLogger",
            ResourceGroupName = resourceGroup.Name,
            ApiManagementName = apimInternal.Name,
            Eventhub = new LoggerEventhubArgs
            {
                Name = eventHub.Name,
                ConnectionString = eventHubNamespace.DefaultPrimaryConnectionString
            }
        });
        
        var agwName = NamingConvention.GetAGWName("api", environment);
        var agwPublicIp = new PublicIp("agw-api-pip", new PublicIpArgs
        {
            Name = NamingConvention.GetPublicIpName("agw-api", environment),
            ResourceGroupName = resourceGroup.Name,
            Sku = "Standard",
            AllocationMethod = "Static",
            DomainNameLabel = agwName
        });
        
        var agwMI = new UserAssignedIdentity("agw-mi", new UserAssignedIdentityArgs
        {
            Name = NamingConvention.GetManagedIdentityName("agw", environment),
            ResourceGroupName = resourceGroup.Name
        });
        
        var apiAgw = new ApplicationGateway("agw-api", new ApplicationGatewayArgs
        {
            Name = agwName,
            ResourceGroupName = resourceGroup.Name,
            Identity = new ApplicationGatewayIdentityArgs
            {
                Type = "UserAssigned",
                IdentityIds = agwMI.Id
            },
            Sku = new ApplicationGatewaySkuArgs
            {
                Name = "WAF_v2",
                Tier = "WAF_v2",
                Capacity = 1
            },
            SslCertificates = new []
            {
                new ApplicationGatewaySslCertificateArgs
                {
                    Name = "gateway-listener",
                    KeyVaultSecretId = config.Require("certificate.keyvaultid")
                }
            },
            FrontendPorts = new []
            {
                new ApplicationGatewayFrontendPortArgs
                {
                    Name = "port443",
                    Port = 443
                },
                new ApplicationGatewayFrontendPortArgs
                {
                    Name = "port80",
                    Port = 80
                }
            },
            GatewayIpConfigurations = new []
            {
                new ApplicationGatewayGatewayIpConfigurationArgs
                {
                    Name = "appGatewayIpConfig",
                    SubnetId = agwSubnet.Id
                }
            },
            FrontendIpConfigurations = new []
            {
                new ApplicationGatewayFrontendIpConfigurationArgs
                {
                    Name = "frontendIP",
                    PublicIpAddressId = agwPublicIp.Id
                }
            },
            HttpListeners = new []
            {
                new ApplicationGatewayHttpListenerArgs
                {
                    Name = "default",    
                    FrontendIpConfigurationName = "frontendIP",
                    FrontendPortName = "port443",
                    Protocol = "Https",
                    HostName = "api.iac-labs.com",
                    RequireSni = true,
                    SslCertificateName = "gateway-listener"
                }
            },
            BackendAddressPools = new[]
            {
                new ApplicationGatewayBackendAddressPoolArgs
                {
                    Name = "apim",
                    IpAddresses = apimInternal.PrivateIpAddresses //config.RequireSecret("apim.backend.ip")
                }
            },
            Probes = new[]
            {
                new ApplicationGatewayProbeArgs
                {
                    Name = "apim-probe-default",
                    Protocol = "Https",
                    Path = "/status-0123456789abcdef",
                    Host = "api.iac-labs.com",
                    Interval = 30,
                    Timeout = 120,
                    UnhealthyThreshold = 8,
                    PickHostNameFromBackendHttpSettings = false,
                    MinimumServers = 0
                }
            },
            BackendHttpSettings = new []
            {
                new ApplicationGatewayBackendHttpSettingArgs
                {
                    Name = "apim-settings-default",
                    Port = 443,
                    Protocol = "Https",
                    CookieBasedAffinity = "Disabled",
                    PickHostNameFromBackendAddress = false,
                    RequestTimeout = 30,
                    ProbeName = "apim-probe-default"
                }
            },
            RequestRoutingRules = new[]
            {
                new ApplicationGatewayRequestRoutingRuleArgs
                {
                    Name = "default",
                    RuleType = "Basic",
                    HttpListenerName = "default",
                    BackendAddressPoolName = "apim",
                    BackendHttpSettingsName = "apim-settings-default"
                }
            }
        });
        
        // var la = new AnalyticsWorkspace("la", new AnalyticsWorkspaceArgs
        // {
        //     Name = NamingConvention.GetLogAnalyticsName(environment),
        //     ResourceGroupName = resourceGroup.Name,
        //     Location = resourceGroup.Location,
        //     Sku = "PerGB2018"
        // });        
        
        // var firewallName = NamingConvention.GetFirewallName(environment);
        //
        // var afwPublicIp = new PublicIp("afw-pip", new PublicIpArgs
        // {
        //     Location = resourceGroup.Location,
        //     ResourceGroupName = resourceGroup.Name,
        //     AllocationMethod = "Static",
        //     Sku = "Standard",
        //     DomainNameLabel = firewallName
        // });
        //
        // var afw = new Firewall("afw", new FirewallArgs
        // {
        //     Name = firewallName,
        //     Location = resourceGroup.Location,
        //     ResourceGroupName = resourceGroup.Name,
        //     IpConfigurations = 
        //     {
        //         new FirewallIpConfigurationArgs
        //         {
        //             Name = "configuration",
        //             SubnetId = firewallSubnet.Id,
        //             PublicIpAddressId = afwPublicIp.Id,
        //         },
        //     },
        // });
        //
        // var afwNatRuleCollection = new FirewallNatRuleCollection("apim-dnat", new FirewallNatRuleCollectionArgs
        // {
        //     Name = "apim-dnat",
        //     AzureFirewallName = afw.Name,
        //     ResourceGroupName = resourceGroup.Name,
        //     Priority = 100,
        //     Action = "Dnat",
        //     Rules = 
        //     {
        //         new FirewallNatRuleCollectionRuleArgs
        //         {
        //             Name = "apim",
        //             SourceAddresses = "*",
        //             DestinationPorts = 
        //             {
        //                 "443",
        //             },
        //             DestinationAddresses = 
        //             {
        //                 afwPublicIp.IpAddress,
        //             },
        //             TranslatedPort = "443",
        //             TranslatedAddress = apimInternal.PrivateIpAddresses.First(),
        //             Protocols = 
        //             {
        //                 "UDP",
        //                 "TCP",
        //             }
        //         }
        //     }
        // });
        
        // var afwDiagnosticSetting = new DiagnosticSetting("afw-diagnostics", new DiagnosticSettingArgs
        // {
        //     Name = "diagnostics",
        //     TargetResourceId = afw.Id,
        //     LogAnalyticsWorkspaceId = la.Id,
        //     Logs = 
        //     {
        //         new DiagnosticSettingLogArgs
        //         {
        //             Category = "AzureFirewallApplicationRule",
        //             Enabled = true,
        //             RetentionPolicy = new DiagnosticSettingLogRetentionPolicyArgs
        //             {
        //                 Enabled = false
        //             }
        //         },
        //         new DiagnosticSettingLogArgs
        //         {
        //             Category = "AzureFirewallNetworkRule",
        //             Enabled = true,
        //             RetentionPolicy = new DiagnosticSettingLogRetentionPolicyArgs
        //             {
        //                 Enabled = false
        //             }
        //         },
        //         new DiagnosticSettingLogArgs
        //         {
        //             Category = "AzureFirewallDnsProxy",
        //             Enabled = true,
        //             RetentionPolicy = new DiagnosticSettingLogRetentionPolicyArgs
        //             {
        //                 Enabled = false,
        //             }
        //         }
        //     },
        //     Metrics = 
        //     {
        //         new DiagnosticSettingMetricArgs
        //         {
        //             Category = "AllMetrics",
        //             Enabled = true,
        //             RetentionPolicy = new DiagnosticSettingMetricRetentionPolicyArgs
        //             {
        //                 Enabled = false,
        //             }
        //         }
        //     }
        // });        
    }
}
