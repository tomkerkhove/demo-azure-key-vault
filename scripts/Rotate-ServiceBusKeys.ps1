$connectionName = "AzureRunAsConnection"

try
{
    # Get the connection "AzureRunAsConnection "
    $servicePrincipalConnection = Get-AutomationConnection -Name $connectionName         

    "Logging in to Azure..."
    Add-AzureRmAccount `
        -ServicePrincipal `
        -TenantId $servicePrincipalConnection.TenantId `
        -ApplicationId $servicePrincipalConnection.ApplicationId `
        -CertificateThumbprint $servicePrincipalConnection.CertificateThumbprint

    "Login complete."
}
catch {
    if (!$servicePrincipalConnection)
    {
        $ErrorMessage = "Connection $connectionName not found."
        throw $ErrorMessage
    } else{
        Write-Error -Message $_.Exception
        throw $_.Exception
    }
}

function Roll-ServiceBusKey($resourceGroupName, $vaultName, $serviceBusNamespaceName, $serviceBusAccessPolicyName, $secretName)
{
    Write-Output "Rolling authentication keys for Service Bus namespace '$serviceBusNamespaceName' with access policy '$serviceBusAccessPolicyName'"

    # Roll secondary key
    $policyKeys = New-AzureRmServiceBusNamespaceKey -AuthorizationRuleName $serviceBusAccessPolicyName -NamespaceName $serviceBusNamespaceName -RegenerateKeys SecondaryKey -ResourceGroup $resourceGroupName
    Write-Output "Secondary key rolled"

    # Update secret in Key Vault with new secondary key
    $secretValue = ConvertTo-SecureString $policyKeys.SecondaryConnectionString -AsPlainText -Force
    $secret = Set-AzureKeyVaultSecret -vaultName $vaultName -Name $secretName -secretValue $secretValue

    # Roll primary key
    $policyKeys = New-AzureRmServiceBusNamespaceKey -AuthorizationRuleName $serviceBusAccessPolicyName -NamespaceName $serviceBusNamespaceName -RegenerateKeys PrimaryKey -ResourceGroup $resourceGroupName
    Write-Output "Primary key rolled"

    # Update secret in Key Vault with new primary key
    $secretValue = ConvertTo-SecureString $policyKeys.PrimaryConnectionString -AsPlainText -Force
    $secret = Set-AzureKeyVaultSecret -vaultName $vaultName -Name $secretName -secretValue $secretValue

    Write-Output "Authentication keys rolled for Service Bus namespace '$serviceBusNamespaceName' with access policy '$serviceBusAccessPolicyName'"
}

# Example of rolling keys
Roll-ServiceBusKey -resourceGroupName 'secure-applications' -vaultName 'secure-applications'
                    -serviceBusNamespaceName 'secure-applications' -serviceBusAccessPolicyName 'API'
                    -secretName 'Messaging-ConnectionString'                   