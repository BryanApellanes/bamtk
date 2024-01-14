# Bam Docs
The `bam application management` tool provides a console/terminal/shell interface used to manage applications built on the `bam application management framework`.

## Set up
### Prerequisites
- git
- docker

### optional
- protoc - for protocol buffers support

### dotnet
```
dotnet tool install -g bam
```

### nodejs
```
npm instgall bam
```

### python
```
pip install bam
```

## Sign Up
```
bam signup --email {emailaddress}
```

> NOTES: <br />
alias for: <br /> &nbsp;
bam org create --email {emailaddress} [--username {username}]

## Authentication
```
bam authenticate
```

> NOTES: <br /> &nbsp;
alias for: bam request authentication --user {username}

## Show
```
bam show
```

> NOTES: <br />&nbsp;
This command should show the current configuration.  Define a configuration object that is yaml serialized to .bam/.config

## Whoami
```
bam whoami
```

## Create Application
```
bam app create --name {your-app-name}
```

## Main.md

```bam://*/data

typeName: Item
id: ulong
name: string
description: 
```
