$moderatorAppName = "Moderator API"
$connectorAppName = "Moderator Connector"


# Moderator API AppReg

$requiredResourceAccess = @(
  @{
    ResourceAppId  = "00000003-0000-0000-c000-000000000000"; # Microsoft Graph
    ResourceAccess = @(
      @{
        Id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d"; # User.Read
        Type = "Scope"
      }
    );
  }
)

$moderatorApp = New-AzADApplication -DisplayName $moderatorAppName -AvailableToOtherTenants $false -RequiredResourceAccess $requiredResourceAccess 

$identifierUri = @( "api://$($moderatorApp.AppId)" )
$apiOauth2PermissionScope_id = New-Guid
$api = @{
  Oauth2PermissionScope       = @(
    @{
      Id                      = $apiOauth2PermissionScope_id;
      Value                   = "access_as_user";
      IsEnabled               = $true;
      Type                    = "User";
      AdminConsentDescription = "Access Moderator API";
      AdminConsentDisplayName = "Access Moderator API";
      UserConsentDescription  = "Access Moderator API";
      UserConsentDisplayName  = "Access Moderator API";
    }
  );
  RequestedAccessTokenVersion = 2
}

Set-AzADApplication -ApplicationId $moderatorApp.AppId -IdentifierUri $identifierUri -Api $api
New-AzADServicePrincipal -ApplicationId $moderatorApp.AppId

Write-Host "Moderator API registration"
Write-Host ("ClientId: {0}" -f $moderatorApp.AppId)
Write-Host ("TenantId: {0}" -f (Get-AzContext).Tenant.Id)
Write-Host

$moderatorApp = Get-AzADApplication -ObjectId "f66b4f71-0290-4908-bb85-c2b36429a8a8"

# Connector AppReg

$connectorReplyUrls = @(
  "https://example.com/update-this"
)

$connectorRequiredResourceAccess = @(
  @{
    ResourceAppId  = $moderatorApp.AppId
    ResourceAccess = @(
      @{
        Id   = $apiOauth2PermissionScope_id
        Type = "Scope"
      }
    );
  }
)

$connectorApi = @{ RequestedAccessTokenVersion = 2 }

$connectorApp = New-AzADApplication -DisplayName $connectorAppName `
                                    -AvailableToOtherTenants $false `
                                    -Api $connectorApi `
                                    -ReplyUrls $connectorReplyUrls `
                                    -RequiredResourceAccess $requiredResourceAccess

$connectorAppCredential = New-AzADAppCredential  -ApplicationId $connectorApp.AppId

Write-Host "Connector registration"
Write-Host ("Client Id:  {0}" -f $connectorApp.AppId)
Write-Host ("Client Secret: {0}" -f $connectorAppCredential.SecretText)
Write-Host ("Resource URL: {0}" -f $identifierUri)
Write-Host
