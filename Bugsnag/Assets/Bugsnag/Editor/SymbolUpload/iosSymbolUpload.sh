#!/bin/bash

# Iterate through all input files
for ((i=0; i<SCRIPT_INPUT_FILE_COUNT; i++))
do
    # Dynamically get the input file variable name
    INPUT_FILE_VAR="SCRIPT_INPUT_FILE_$i"
    INPUT_FILE=${!INPUT_FILE_VAR}

    # Extract path up to and including BugsnagUnity.app.dSYM
    DSYM_PATH=$(echo "$INPUT_FILE" | sed 's#/Contents.*##')

    echo "Uploading dSYM: $DSYM_PATH"

    # Upload the dSYM file
    <CLI_COMMAND>
done
