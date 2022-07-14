# CoffeeBackup
Linux server backup utility with Storj storage and Docker definitions.

This application was tested and developed for Ubuntu 20.04 but should work for other Linux distributions.


## Installation

## Setting up prerequisites
- [Storj account](https://storj.io/)
- [Azure account](https://azure.microsoft.com/en-us/free/) (Optional, for securely cleaning up old backups)
- [Docker](https://www.docker.com/)
- [Docker Compose plugin](https://docs.docker.com/compose/install/)

## Setting up Storj
2. Create a Storj bucket [here](https://eu1.storj.io/buckets/creation) and store the passphrase securely.
3. Create two "API Access" access grants for the Storj bucket [here](https://eu1.storj.io/access-grants) and store the addresses & keys.
	- Name: 'SERVERNAME-backup-service' with Permissions: `Write`, `List`
		- Add `Delete` permission if you want to auto-clean backups without Azure Functions.
	- Name: 'SERVERNAME-backup-cleanup' with Permissions: `Delete`, `List`
		- Only when you intend to auto-clean backups with Azure Functions.

## Setting up the backup service
Depending on your needs, you can set up a docker-compose.yml file with environment variables or use an appsettings.json file.  
For this guide, we'll use an appsettings file.

1. SSH into the server
2. nano /etc/coffeebackup/appsettings.json
3. Paste in the contents of [appsettings.service.example.json](https://github.com/NotCoffee418/CoffeeBackup/appsettings/appsettings.service.example.json])
4. CTRL+X to save and exit out.


## Auto-cleaning old backups
### Option 1: Using Azure functions
With this option we will set up an Azure function to run periodically and delete any outdated backups.
The cost of this function is (to be tested) about $0.02 per month.

### Option 2: Through the backup service (NOT RECOMMENDED)
You can also clean up old backups using the backup service directly.
This is easier to set up, but not recommended in the event the server is compromised, a malicious actor can also remove backups.

1. Modify the storage bucked to have delete permissions.
2. Add the following environment variable to the docker-compose.yml, replacing the value with the desired number of days to keep backups.
```yaml
environment:
	- CLEANUP_BACKUP_AFTER_DAYS=30
```