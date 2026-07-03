variable "subscription_id" {
  type        = string
  description = "Azure subscription ID (RD-Box)."
  default     = "acf84f87-9597-43fd-9994-761c094389fa"
}

variable "tenant_id" {
  type        = string
  description = "Entra External ID (CIAM) tenant ID."
  default     = "9af8af7b-10ee-4bd5-b71c-20daa8e37878"
}

variable "location" {
  type        = string
  description = "Azure region for Key Vault."
  default     = "eastus2"
}

variable "resource_group_name" {
  type        = string
  description = "Existing resource group name."
  default     = "rdbox-rg"
}

variable "key_vault_name" {
  type        = string
  description = "Globally unique Key Vault name."
  default     = "rdbox-kv"
}

variable "entra_app_display_name" {
  type        = string
  description = "Display name for the box-web app registration."
  default     = "box-web"
}

variable "entra_authority" {
  type        = string
  description = "OIDC authority URL for the CIAM tenant."
  default     = "https://rdbox.ciamlogin.com/9af8af7b-10ee-4bd5-b71c-20daa8e37878/v2.0"
}

variable "oidc_redirect_uris" {
  type        = list(string)
  description = "Registered redirect URIs for the box-web OIDC client."
  default = [
    "http://localhost/api/users/signin-oidc",
  ]
}

variable "client_secret_end_date" {
  type        = string
  description = "End date for the app registration client secret (RFC3339)."
  default     = "2027-07-01T00:00:00Z"
}

variable "deployer_object_id" {
  type        = string
  description = "Object ID of the user or service principal running Terraform (Key Vault Secrets Officer)."
  default     = ""
}
