# azs-utils - Azure Storage Account Utility

`azs-utils` is a utility for Azure storage accounts that goes in an extra step beyond what `az-cli` or `azcopy` can do. Learn more at https://endpointsystems.com/blog/introducing-azs-util. 

## Getting Started
Set the `AZURE_STORAGE_CONNECTION_STRING` environment variable to the connection string of the storage account you wish to work on.

## Container Commands

```
Container storage commands

Usage: azs-util containers [command] [options]

Options:
  -?|-h|--help  Show help information

Commands:
  delete        deletes containers from the storage account.
  list          List blob storage container names.
  size          get container size
```

The `delete` command will allow for single container deletion or a bulk delete of all containers in your storage account. 

The `list` command gives you a clean, simple list of container names without the CLI jiu-jitsu of a [JMESPath Query](http://jmespath.org/). 

The `size` command iterates through all blobs, providing an object count and size for each container, and summary info at the end. 

## Table Commands
```
Table commands

Usage: azs-util table [command] [options]

Options:
  -?|-h|--help  Show help information

Commands:
  rename-pk     Rename a PartitionKey to something else.
  list-pk       List all unique PartitionKeys.
  list-rk       List all RowKeys for a given PartitionKey.

Run 'table [command] -?|-h|--help' for more information about a command.

```
The `rename-pk` command allows you to rename a `PartitionKey` in a table. 
