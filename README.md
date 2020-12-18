# README for Pisces 5.3 and up!

This application calls low frequency variants, on linux or windows. It will run on tumor-only samples, and search for SNVs, MNVs, and small indels. It takes in .bams and generates .vcf or .gvcf files. It is included with most Illumina sequencing machines, starting with the MiSeq in 2011, and various BaseSpace workflows. The caller can also be run as a standalone program.  

Please cite us at: 
Tamsen Dunn, Gwenn Berry, Dorothea Emig-Agius, Yu Jiang, Serena Lei, Anita Iyer, Nitin Udar, Han-Yu Chuang, Jeff Hegarty, Michael Dickover, Brandy Klotzle, Justin Robbins, Marina Bibikova, Marc Peeters, Michael Strömberg, Pisces: an accurate and versatile variant caller for somatic and germline next-generation sequencing data, Bioinformatics, Volume 35, Issue 9, 1 May 2019, Pages 1579–1581, https://doi.org/10.1093/bioinformatics/bty849

POC: 
[Tamsen Dunn](https://www.linkedin.com/in/tamsen-dunn-7340145) and
[Gwenn Berry](https://www.linkedin.com/in/gwenn-berry-43071939)

Quick start: [Example command lines](https://github.com/tamsen/Pisces/wiki/Pisces-Quick-Start-5.3.0)

# License
Pisces source code is provided under the GPLv3 license. Pisces includes third party packages provided under other open source licenses, please see COPYRIGHT.txt for additional details.

# System requirements

For v5.3.0 and up,  64 bit Linux is required. 
Or the software can be recompiled for a target of your choice. See the build instructions for more details. 

For older versions, see the original [illumina fork](https://github.com/Illumina/Pisces)


# Running from a binary distribution

## For v5.3 and up:

The uncompressed binary is ready to go as a stand alone. Uncompress with "tar xvzf file.tar.gz".  No other software is required. Only the linux OS is supported, although it is possible to recompile for other targets.

Example command line for v5.3 and up:

Pisces -bam /my/path/to/TestData/example_S1.bam -g /my/path/to/WholeGenomeFasta 

Example qsub cmd to a grid cluster:

echo "/here/is/Pisces -bam /my/path/to/TestData/example_S1.bam -g /my/path/to/WholeGenomeFasta "  | qsub -N PISCESJob -pe threaded 16-20 -M you@yoursmtp.com -m eas

It is necessary to supply a reference genome following the -g argument. Reference genomes may be downloaded from illumina's website at: http://support.illumina.com/sequencing/sequencing_software/igenome.ilmn . Pisces will run on non-human genomes. The genome build should match the genome to which the bam was aligned. The file "GenomeSize.xml" is required in the folder alongside the genome file. This is standard with most illumina genomes, but can be created from the fasta file as needed using CreateGenomeSizeFile utility in the Pisces software suite.

## For v5.2 and below:

In previous versions, both linux and windows were supported. ".net core 2.2" must be installed, and then the uncompressed binary is ready to go. See the original [illumina fork](https://github.com/Illumina/Pisces) for more details.

Example Command line for v5.2 and below:

windows:

dotnet Pisces.dll -bam C:\my\path\to\TestData\example_S1.bam -g C:\my\path\to\WholeGenomeFasta

linux:

dotnet Pisces.dll -bam /my/path/to/TestData/example_S1.bam -g /my/path/to/WholeGenomeFasta 


# Build instructions

The component algorithms are intended for developers to re-use and improve them. This version is not commercially supported and provided as is under the GNU GENERAL PUBLIC LICENSE. For first time use, we recommend testing with the example in the "testdata" folder.

If you wish to build Pisces 5.3 and up, I recommend
1) A linux machine
2) Install .NET Core SDK 2.2.402 (https://aka.ms/dotnet-download)
3) Install Visual Studio Code (if you plan to do dev or debugging).
4) Clone the https://github.com/tamsen/Pisces repo

To build all solutions, navigate to Pisces/src/scripts in your cloned repo and run "python build_standalone.py" .  To rebuild a single project, do "dotnet build /path/to/desired/project.csprj" .

For dev, open VisualStudioCode and from inside VisualStudioCode open the Pisces folder from the dowloaded github clone. Allow it to update dependances. You should be good to go.

For older versions of Pisces (5.2 and below) a Windows box or virtual environment is required for dev work. See instructions on the original [illumina fork](https://github.com/Illumina/Pisces)


# User Guide
https://github.com/tamsen/Pisces/wiki

# FAQ
https://github.com/tamsen/Pisces/wiki/Frequently-Asked-Questions

# Support

for those using an illumina pipeline:  techsupport@illumina.com

Pisces-specific:

[Tamsen Dunn](https://www.linkedin.com/in/tamsen-dunn-7340145) and
[Gwenn Berry](https://www.linkedin.com/in/gwenn-berry-43071939)

If you are using Pisces, feel free to introduce yourself!

