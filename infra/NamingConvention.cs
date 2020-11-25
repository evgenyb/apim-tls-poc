public static class NamingConvention
{
    private const string BasePrefix = "iac";
    public static string GetResourceGroupName(string environment)
    {
        return $"{BasePrefix}-{environment}-rg";
    }

    public static string GetVNetName(string environment)
    {
        return $"{BasePrefix}-{environment}-vnet";
    }

    public static string GetAGWName(string agwName, string environment)
    {
        return $"{BasePrefix}-{environment}-{agwName}-agw";
    }

    public static string GetPublicIpName(string pipName, string environment)
    {
        return $"{BasePrefix}-{environment}-{pipName}-pip";
    }

    public static string GetManagedIdentityName(string miName, string environment)
    {
        return $"{BasePrefix}-{environment}-{miName}-mi";
    }

    public static string GetApimName(string environment)
    {
        return $"{BasePrefix}-{environment}-apim";
    }

    public static string GetFirewallName(string environment)
    {
        return $"{BasePrefix}-{environment}-afw";
    }

    public static string GetAksName(string environment)
    {
        return $"{BasePrefix}-{environment}-aks";
    }

    public static string GetAppInsightName(string environment)
    {
        return $"{BasePrefix}-{environment}-ai";
    }

    public static string GetLogAnalyticsName(string environment)
    {
        return $"{BasePrefix}-{environment}-la";
    }

    public static string GetNetworkSecurityGroup(string nsgName, string environment)
    {
        return $"{BasePrefix}-{environment}-{nsgName}-nsg";
    }
}