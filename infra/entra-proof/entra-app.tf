resource "azuread_application" "box_web" {
  display_name = var.entra_app_display_name

  # Entra External ID (CIAM) rejects v1 access tokens; require v2.
  api {
    requested_access_token_version = 2
  }

  web {
    redirect_uris = var.oidc_redirect_uris
  }

  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000" # Microsoft Graph

    resource_access {
      id   = "37f7f235-527c-4136-accd-4a02d197296e" # openid
      type = "Scope"
    }

    resource_access {
      id   = "14dad69e-098b-42c7-925c-94f88b96a993" # profile
      type = "Scope"
    }

    resource_access {
      id   = "64a6cdd6-aab1-4aaf-94b8-3cc8405e90d0" # email
      type = "Scope"
    }
  }
}

resource "azuread_application_password" "box_web" {
  application_id = azuread_application.box_web.id
  display_name   = "terraform-managed"
  end_date       = var.client_secret_end_date
}

# Enterprise application (service principal). Required for the app to appear in
# the External ID user-flow "Add application" screen and to be assignable.
resource "azuread_service_principal" "box_web" {
  client_id = azuread_application.box_web.client_id
}

resource "azurerm_key_vault_secret" "auth_entra_client_secret" {
  name         = "auth-entra-client-secret"
  value        = azuread_application_password.box_web.value
  key_vault_id = azurerm_key_vault.box.id

  depends_on = [azurerm_role_assignment.deployer_secrets_officer]
}

resource "azurerm_key_vault_secret" "auth_entra_client_id" {
  name         = "auth-entra-client-id"
  value        = azuread_application.box_web.client_id
  key_vault_id = azurerm_key_vault.box.id

  depends_on = [azurerm_role_assignment.deployer_secrets_officer]
}

resource "azurerm_key_vault_secret" "auth_entra_authority" {
  name         = "auth-entra-authority"
  value        = var.entra_authority
  key_vault_id = azurerm_key_vault.box.id

  depends_on = [azurerm_role_assignment.deployer_secrets_officer]
}
