#!/usr/bin/env bash

##########################################################################
# This is the Cake bootstrapper script for Linux and OS X.
# This file was downloaded from https://github.com/cake-build/resources
# Feel free to change this file to fit your needs.
##########################################################################

# Define directories.
CAKE_VERSION=0.32.1
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/tools
TOOLS_PROJ=$TOOLS_DIR/tools.csproj
CAKE_DLL=$TOOLS_DIR/Cake.CoreCLR.$CAKE_VERSION/cake.coreclr/$CAKE_VERSION/Cake.dll

# Make sure the tools folder exist.
if [ ! -d "$TOOLS_DIR" ]; then
  mkdir -p "$TOOLS_DIR"
fi

# Define default arguments.
SCRIPT="build.cake"
CAKE_ARGUMENTS=()

# Parse arguments.
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        --) shift; CAKE_ARGUMENTS+=("$@"); break ;;
        *) CAKE_ARGUMENTS+=("$1") ;;
    esac
    shift
done

# Make sure the tools folder exist.
if [ ! -d "$TOOLS_DIR" ]; then
  mkdir "$TOOLS_DIR"
fi

###########################################################################
# INSTALL CAKE
###########################################################################

if [ ! -f "$CAKE_DLL" ]; then
    echo "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>netcoreapp2.2</TargetFramework></PropertyGroup></Project>" > $TOOLS_PROJ
    dotnet add $TOOLS_PROJ package cake.coreclr -v $CAKE_VERSION --package-directory $TOOLS_DIR/Cake.CoreCLR.$CAKE_VERSION
fi

# Make sure that Cake has been installed.
if [ ! -f "$CAKE_DLL" ]; then
    echo "Could not find Cake.exe at '$CAKE_DLL'."
    exit 1
fi

###########################################################################
# RUN BUILD SCRIPT
###########################################################################

# Start Cake
exec dotnet "$CAKE_DLL" $SCRIPT "${CAKE_ARGUMENTS[@]}"
