# azs-util - Azure Storage Account Utility

`azs-util` is a utility for Azure storage accounts that goes in an extra step beyond what `az-cli` or `azcopy` can do. Learn more at https://endpointsystems.com/blog/introducing-azs-util. 

## Getting Started
Set the `AZURE_STORAGE_CONNECTION_STRING` environment variable to the connection string of the storage account you wish to work on, or use the `cs` option to pass it in on the command line.

For commands where another storage account is involved, set the `AZURE_STORAGE_CONNECTION_DEST` environment variable to the destination storage account you wish to target, or use the `ds` option to pass it in on the command line.

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
  cpc           Copy one column to another (new) column. Similar to renaming except the original exists.
  cpt           Copy a table from one storage account to another.
  dc            Delete a column in your table.
  dt            Delete a table.
  et            Empty a table >= Timestamp (simple date) or use your own custom OData where clause
  rename-pk     Rename a PartitionKey to something else.
  list-pk       List all unique PartitionKeys.
  list-rk       List all RowKeys for a given PartitionKey.

Run 'table [command] -?|-h|--help' for more information about a command.

```
