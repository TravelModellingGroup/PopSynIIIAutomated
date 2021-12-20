# PopSynIIIAutomated

PopSynIII Automated is designed to automate TMG's implementation of PopSynIII.  This program is designed for internal use at the Travel Modelling Group.
Documentation can be found in the TMG's documentation site at [PopSynIII](https://tmg.utoronto.ca/doc/1.6/gtamode/user_guide/PopSynIII/index.html).

PopSynIII Automated is licensed under the GPLv3 license.  You can find the full details
of the license in the LICENSE file that came with PopSynIII Automated.

### Building

This repository is designed to be testable out of the box.  You can compile and
run its unit test given the following two commands.

> dotnet build -c Release

> dotnet test -c Release

The code is broken into two projects, PopSynIIIAutomated
and PopSynIIIAutomatedTest.  These projects can be found in the src and test
directories respectively.  Currently PopSynIIIAutomated is setup to run with
WPF in the future we might break the core logic out of this library to help
make it compatible on additional platforms.

The unit tests come with some small test input files.  Constructing the full
input file directory is out of the scope of this documentation.  Please refer
to the [user guide](https://tmg.utoronto.ca/doc/1.6/gtamode/user_guide/PopSynIII/index.html)
to learn how to construct your own inputs.