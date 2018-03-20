# Sample - Azure Key Vault
[![Build Status](https://travis-ci.org/tomkerkhove/sample-azure-key-vault.svg?branch=master)](https://travis-ci.org/tomkerkhove/sample-azure-key-vault)

## Overview
This repo contains several scenarios around Azure Key Vault:

1. Acquiring & setting secrets with basic Azure AD authentication
2. Acquiring & setting secrets with managed Service Identity authentication
3. Sending message to Service Bus with connection string in Key Vault. Secret is being stored in memory caching for performance optimizations.