using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Azure.Network;
using Pulumi.Azure.Network.Inputs;

public static class ConfigExtensions
{
    public static string GetSubnetAddressRange(this Config self, string subnetName)
    {
        return self.Require($"vnet.subnets.{subnetName}.addressPrefixes");
    }
}

    public class APIMNSG
    {
        public static NetworkSecurityGroup Get(string environment, ResourceGroup resourceGroup, Config config)
        {
            var apimSubnetAddressPrefix = config.GetSubnetAddressRange("apim");
            var aksSubnetAddressPrefix = config.GetSubnetAddressRange("aks");
            var agwSubnetAddressPrefix = config.GetSubnetAddressRange("agw");
            
            return new NetworkSecurityGroup("apim-nsg", new NetworkSecurityGroupArgs
            {
                Name = NamingConvention.GetNetworkSecurityGroup("apim", environment),
                ResourceGroupName = resourceGroup.Name,
                SecurityRules = new[]
                {
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "ALB-ALL-IN-ALLOW",
                        Description = "Inbound Traffic from ALB",
                        Priority = 100,
                        Direction = "Inbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = "AzureLoadBalancer",
                        SourcePortRange = "*",
                        DestinationAddressPrefix = apimSubnetAddressPrefix,
                        DestinationPortRange = "*"
                    }, 
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "AGW-T443-IN-ALLOW",
                        Description = "Inbound Traffic from AGW",
                        Priority = 101,
                        Direction = "Inbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = agwSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = apimSubnetAddressPrefix,
                        DestinationPortRange = "443"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "INT-T443-IN-ALLOW",
                        Description = "Inbound Traffic from Internet",
                        Priority = 102,
                        Direction = "Inbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = "Internet",
                        SourcePortRange = "*",
                        DestinationAddressPrefix = apimSubnetAddressPrefix,
                        DestinationPortRange = "443"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "AZAPIM-T3443-IN-ALLOW",
                        Description = "Management endpoint for Azure portal and Powershell",
                        Priority = 103,
                        Direction = "Inbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = "ApiManagement",
                        SourcePortRange = "*",
                        DestinationAddressPrefix = apimSubnetAddressPrefix,
                        DestinationPortRange = "3443"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "AKS-T443-IN-ALLOW",
                        Description = "Inbound Traffic from aks",
                        Priority = 104,
                        Direction = "Inbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = aksSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = apimSubnetAddressPrefix,
                        DestinationPortRange = "443"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "VNET-T6381-6383-IN-ALLOW",
                        Description = "Access Azure Cache for Redis Instances between RoleInstances",
                        Priority = 105,
                        Direction = "Inbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = "VirtualNetwork",
                        SourcePortRange = "*",
                        DestinationAddressPrefix = apimSubnetAddressPrefix,
                        DestinationPortRange = "6381-6383"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "AKS-T80-OUT-ALLOW",
                        Description = "Access from APIM to AKS",
                        Priority = 100,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = aksSubnetAddressPrefix,
                        DestinationPortRange = "80"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "AKS-T443-OUT-ALLOW",
                        Description = "Access from APIM to AKS",
                        Priority = 101,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = aksSubnetAddressPrefix,
                        DestinationPortRange = "443"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "INT-T443-OUT-ALLOW",
                        Description = "Outbound Traffic to Internet",
                        Priority = 102,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "Internet",
                        DestinationPortRange = "443"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "SQL-T1433-OUT-ALLOW",
                        Description = "Access to Azure SQL endpoints",
                        Priority = 103,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "SQL",
                        DestinationPortRange = "1433"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "INT-T9350-9354-OUT-ALLOW",
                        Description = "Outbound Traffic to Internet",
                        Priority = 104,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "Internet",
                        DestinationPortRange = "9350-9354"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "EH-T5672-OUT-ALLOW",
                        Description = "Dependency for Log to Event Hub policy and monitoring agent",
                        Priority = 105,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "EventHub",
                        DestinationPortRange = "5672"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "INT-T5671-OUT-ALLOW",
                        Description = "Outbound Traffic to Internet",
                        Priority = 106,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "Internet",
                        DestinationPortRange = "5671"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "INT-T11000-11999-OUT-ALLOW",
                        Description = "Outbound Traffic to Internet",
                        Priority = 107,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "Internet",
                        DestinationPortRange = "11000-11999"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "INT-T14000-14999-OUT-ALLOW",
                        Description = "Outbound Traffic to Internet",
                        Priority = 108,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "Internet",
                        DestinationPortRange = "14000-14999"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "INT-U123-OUT-ALLOW",
                        Description = "Outbound Traffic to Internet",
                        Priority = 109,
                        Direction = "Outbound",
                        Protocol = "Udp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "Internet",
                        DestinationPortRange = "123"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "INT-U1886-OUT-ALLOW",
                        Description = "Outbound Traffic to Internet",
                        Priority = 110,
                        Direction = "Outbound",
                        Protocol = "Udp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "Internet",
                        DestinationPortRange = "1886"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "INT-T1886-OUT-ALLOW",
                        Description = "Outbound Traffic to Internet",
                        Priority = 111,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "Internet",
                        DestinationPortRange = "1886"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "STRG-T445-OUT-ALLOW",
                        Description = "Dependency on Azure File Share for GIT",
                        Priority = 112,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "Storage",
                        DestinationPortRange = "445"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "STRG-T12000-OUT-ALLOW",
                        Description = "Dependency on Azure File Share for GIT",
                        Priority = 113,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "Storage",
                        DestinationPortRange = "12000"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "INT-T12000-OUT-ALLOW",
                        Description = "Dependency on Azure File Share for GIT",
                        Priority = 114,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "Internet",
                        DestinationPortRange = "12000"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "INT-T80-OUT-ALLOW",
                        Description = "",
                        Priority = 115,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "Internet",
                        DestinationPortRange = "80"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "AGW-T80-OUT-ALLOW",
                        Description = "",
                        Priority = 116,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = agwSubnetAddressPrefix,
                        DestinationPortRange = "80"
                    },
                    new NetworkSecurityGroupSecurityRuleArgs
                    {
                        Name = "INT-T25-OUT-ALLOW",
                        Description = "Connect to SMTP Relay for sending e-mails",
                        Priority = 117,
                        Direction = "Outbound",
                        Protocol = "Tcp",
                        Access = "Allow",
                        SourceAddressPrefix = apimSubnetAddressPrefix,
                        SourcePortRange = "*",
                        DestinationAddressPrefix = "Internet",
                        DestinationPortRange = "25"
                    }
                }
            });
        }        
    }