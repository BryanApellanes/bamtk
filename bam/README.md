# bam
The `bam application management` tool provides a console/terminal/shell interface used to manage applications built on the `bam application management framework`.

/* from scratch
## Sign Up
```
bam signup --email user@email.com
```

## Create Application
```
bam app init
```

## Define Data Model
```

```

/* --/

## Schema extraction
To extract a schema from an existing database use the following command:
```
bam db schema --extract --from "<database_connection_string>" --dbType <dbType>
```

Valid dbType values are
- sqllite
- mysql
- postgres
- mysql
- oracle

## Generator
### Code generation

To generate csharp data access code use the following command:
```
bam db generate --type <dao | repo> --from ./sourceDirectory --out ./destinationDirectory
```

### --type
If you only need data access objects specify `dao` as the type argument value.  If you prefer to use the repository api specify `repo` as the type argument value.
### Assembly generation

### --compile
To create an assembly when generating source code, specify the `--compile` argument.  This causes generated code to be compiled to the specified destination directory using the default generated assembly name which is a hash of the schema.

```
bam db generate --type <dao | repo> --from ./sourceDirectory --out ./destinationDirectory --compile
```

### Assembly name
To specify a more friendly human readable assembly name specify a value to the `---compile` command.  The assembly will have a `.dll` extension whether specified or omitted.

```
bam db generate --type <dao | repo> --from ./sourceDirectory --out ./destinationDirectory --compile MyDataAcessObjects
```

### --keepSource
If you wish to keep the source files generated specify the `--keepSource` argument:
```
bam db generate --type <dao | repo> --from ./sourceDirectory --out ./destinationDirectory --compile MyDataAcessObjects --keepSource
```

## Serve
To serve an HTTP based RESTful data access api, use the following commmand:

```
bam db serve --from "<database_connection_string>" --dbType <dbType> --classes "<assembly_path> | <src_directory_path>"
```

### Classes argument
The classes argument specifies where data access object classes are defined.  If an assembly path is specified it is assumed to be 