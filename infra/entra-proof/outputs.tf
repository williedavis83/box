output "entra_client_id" {
  description = "Application (client) ID for box-web."
  value       = azuread_application.box_web.client_id
}

output "entra_authority" {
  description = "OIDC authority URL for the CIAM tenant."
  value       = var.entra_authority
}

output "entra_tenant_id" {
  description = "Entra External ID tenant ID."
  value       = var.tenant_id
}

output "key_vault_uri" {
  description = "Key Vault URI for runtime configuration."
  value       = azurerm_key_vault.box.vault_uri
}

output "key_vault_name" {
  description = "Key Vault resource name."
  value       = azurerm_key_vault.box.name
}

output "key_vault_secret_name_client_secret" {
  description = "Key Vault secret name for the OIDC client secret."
  value       = azurerm_key_vault_secret.auth_entra_client_secret.name
}

output "key_vault_secret_name_client_id" {
  description = "Key Vault secret name for the OIDC client ID."
  value       = azurerm_key_vault_secret.auth_entra_client_id.name
}

output "oidc_redirect_uris" {
  description = "Registered OIDC redirect URIs."
  value       = var.oidc_redirect_uris
}
