data "azurerm_resource_group" "box" {
  name = var.resource_group_name
}

data "azurerm_client_config" "current" {}

locals {
  deployer_object_id = var.deployer_object_id != "" ? var.deployer_object_id : data.azurerm_client_config.current.object_id
}
