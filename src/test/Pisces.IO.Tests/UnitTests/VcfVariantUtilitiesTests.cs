﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pisces.Domain.Models.Alleles;
using Pisces.Domain.Types;
using TestUtilities;
using Xunit;

namespace Pisces.IO.Tests.UnitTests
{
    public class VcfVariantUtilitiesTests
    {

        [Fact]
        public void MapFilterStringTests()
        {
            //happy path tests.

            Assert.Equal(0, VcfVariantUtilities.MapFilterString("PASS").Count);
            Assert.Equal(0, VcfVariantUtilities.MapFilterString("pass").Count);
            Assert.Equal(0, VcfVariantUtilities.MapFilterString(".").Count);
            Assert.Equal(0, VcfVariantUtilities.MapFilterString("").Count);
            Assert.Equal(0, VcfVariantUtilities.MapFilterString(" ").Count);

            Assert.Equal(FilterType.LowVariantQscore, VcfVariantUtilities.MapFilterString("lowq")[0]);
            Assert.Equal(FilterType.LowVariantQscore, VcfVariantUtilities.MapFilterString("q20")[0]);
            Assert.Equal(FilterType.LowVariantQscore, VcfVariantUtilities.MapFilterString("q30")[0]);
            Assert.Equal(FilterType.LowVariantQscore, VcfVariantUtilities.MapFilterString("LowQ")[0]);
            Assert.Equal(FilterType.LowVariantQscore, VcfVariantUtilities.MapFilterString("LowQ500")[0]);
            Assert.Equal(FilterType.LowVariantQscore, VcfVariantUtilities.MapFilterString("LowQual")[0]);

            Assert.Equal(FilterType.PoolBias, VcfVariantUtilities.MapFilterString("pb")[0]);

            Assert.Equal(FilterType.StrandBias, VcfVariantUtilities.MapFilterString("sb")[0]);

            Assert.Equal(FilterType.AmpliconBias, VcfVariantUtilities.MapFilterString("ab")[0]);

            Assert.Equal(FilterType.LowDepth, VcfVariantUtilities.MapFilterString("LOWDP")[0]);
            Assert.Equal(FilterType.LowDepth, VcfVariantUtilities.MapFilterString("lowdp")[0]);
            Assert.Equal(FilterType.LowDepth, VcfVariantUtilities.MapFilterString("lowdepth")[0]);

            Assert.Equal(FilterType.LowVariantFrequency, VcfVariantUtilities.MapFilterString("lowfreq")[0]);
            Assert.Equal(FilterType.LowVariantFrequency, VcfVariantUtilities.MapFilterString("lowvariantfreq")[0]);

            Assert.Equal(FilterType.LowGenotypeQuality, VcfVariantUtilities.MapFilterString("lowgq")[0]);
            Assert.Equal(FilterType.LowGenotypeQuality, VcfVariantUtilities.MapFilterString("gq")[0]);

            Assert.Equal(FilterType.IndelRepeatLength, VcfVariantUtilities.MapFilterString("r8")[0]);
            Assert.Equal(FilterType.IndelRepeatLength, VcfVariantUtilities.MapFilterString("R42")[0]);

            Assert.Equal(FilterType.RMxN, VcfVariantUtilities.MapFilterString("R5x9")[0]);
            Assert.Equal(FilterType.RMxN, VcfVariantUtilities.MapFilterString("R3x2")[0]);

            Assert.Equal(FilterType.MultiAllelicSite, VcfVariantUtilities.MapFilterString("multiallelicsite")[0]);

            Assert.Equal(FilterType.ForcedReport, VcfVariantUtilities.MapFilterString("forcedreport")[0]);

            Assert.Equal(FilterType.NoCall, VcfVariantUtilities.MapFilterString("nc")[0]);


            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("MyCatIsCool")[0]);


            //pathological tests

            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("PAS")[0]);
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("passFoo")[0]);
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("!")[0]);
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("42")[0]);

            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("q")[0]);
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("bq20")[0]);
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("sq30")[0]);

            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("pb3")[0]);
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("4sb")[0]);
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("ab2")[0]);
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("LOWDP500")[0]);

            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("7r8")[0]);
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("r")[0]);

            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("R5Y9")[0]);
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("R3Z2")[0]);

            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("multiallelicsite43")[0]);

            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("4forcedreport")[0]);

            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("ncc")[0]);

            //combinations

            var filterString1 = "lowdepth;lowvariantfreq;gq;r5x9  \t ; blah ; multiallelicsite;foo ";

            var filterString1List = VcfVariantUtilities.MapFilterString(filterString1);

            Assert.Equal(FilterType.LowDepth, filterString1List[0]);
            Assert.Equal(FilterType.LowVariantFrequency, filterString1List[1]);
            Assert.Equal(FilterType.LowGenotypeQuality, filterString1List[2]);
            Assert.Equal(FilterType.RMxN, filterString1List[3]);
            Assert.Equal(FilterType.Unknown, filterString1List[4]);
            Assert.Equal(FilterType.MultiAllelicSite, filterString1List[5]);
            Assert.Equal(FilterType.Unknown, filterString1List[6]);

            //really strange stuff...
 
            Assert.Equal(0, VcfVariantUtilities.MapFilterString("; ; ;").Count);
            Assert.Equal(0, VcfVariantUtilities.MapFilterString("; ; PASS;").Count);   
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString(", ,... , ")[0]);//note, only splits on ";", not "'"
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString(", , , ")[0]);
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("lowdepth, multiallelicsite, lowvariantfreq , gq, r5x9")[0]);
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("(*%.,PASS,q30")[0]);
            Assert.Equal(1, VcfVariantUtilities.MapFilterString("(*%.,PASS,q30").Count);
            Assert.Equal(2, VcfVariantUtilities.MapFilterString("(*%.,;PASS;q30").Count);
            Assert.Equal(FilterType.Unknown, VcfVariantUtilities.MapFilterString("(*%.,;PASS;q30")[0]);
            Assert.Equal(FilterType.LowVariantQscore, VcfVariantUtilities.MapFilterString("(*%.,;PASS;q30")[1]);
  

        }

        [Fact]
        public void IsRMxNTests()
        {
            CheckRMxN("r5x9", 5, 9, true);
            CheckRMxN("R5x9", 5, 9, true);
            CheckRMxN("R5X9", 5, 9, true);
            CheckRMxN("r5x9123", 5, 9123, true);
            CheckRMxN("r5123x9", 5123, 9, true);
            CheckRMxN("r-5123x-9", -5123, -9, true);
            CheckRMxN("r0x0", 0, 0, true);

            CheckRMxN("rr5x9", -1, -1, false);
            CheckRMxN("r5L9", -1, -1, false);
            CheckRMxN("L5r9", -1, -1, false);
            CheckRMxN("r5x9x12", -1, -1, false);
            CheckRMxN("r5xx9", -1, -1, false);
            CheckRMxN("r5.1xx9.0", -1, -1, false);
            CheckRMxN("r5x" + ((long)int.MaxValue + 1), -1, -1, false);
        }

        private static void CheckRMxN(string filter, int expectedM, int expectedN, bool expectedIsRMxN)
        {
            int m = -1;
            int n = -1;
            var worked1 = VcfVariantUtilities.IsRMxN(filter);
            var worked2 = VcfVariantUtilities.IsRMxN(filter, out m, out n);
            Assert.Equal(expectedM, m);
            Assert.Equal(expectedN, n);
            Assert.Equal(expectedIsRMxN, worked1);
            Assert.Equal(expectedIsRMxN, worked2);
        }

        [Fact]
        public void MapGT()
        {
            var numTestsNeeded = 13;

            Assert.Equal(Genotype.Alt12LikeNoCall, VcfVariantUtilities.MapGTString("./.", 2));
            Assert.Equal(Genotype.AltAndNoCall, VcfVariantUtilities.MapGTString("1/.", 1));
            Assert.Equal(Genotype.AltLikeNoCall, VcfVariantUtilities.MapGTString("./.", 1));
            Assert.Equal(Genotype.HeterozygousAlt1Alt2, VcfVariantUtilities.MapGTString("1/2", 2));
            Assert.Equal(Genotype.HeterozygousAltRef, VcfVariantUtilities.MapGTString("0/1", 1));
            Assert.Equal(Genotype.HomozygousAlt, VcfVariantUtilities.MapGTString("1/1", 1));
            Assert.Equal(Genotype.HomozygousRef, VcfVariantUtilities.MapGTString("0/0", 0));
            Assert.Equal(Genotype.RefAndNoCall, VcfVariantUtilities.MapGTString("0/.", 0));
            Assert.Equal(Genotype.RefLikeNoCall, VcfVariantUtilities.MapGTString("./.", 0));
            Assert.Equal(Genotype.HemizygousAlt, VcfVariantUtilities.MapGTString("1", 1));
            Assert.Equal(Genotype.HemizygousRef, VcfVariantUtilities.MapGTString("0", 1));
            Assert.Equal(Genotype.HemizygousNoCall, VcfVariantUtilities.MapGTString(".", 1));

            //sanity check we covered all the possibilities.
            Assert.Equal(Enum.GetValues(typeof(Genotype)).Length, numTestsNeeded);
        }


        [Fact]
        public void CompareSubstring()
        {
            Assert.True(VcfVariantUtilities.CompareSubstring("ABC", "ABC", 0));
            Assert.True(VcfVariantUtilities.CompareSubstring("C", "ABC", 2));
            Assert.False(VcfVariantUtilities.CompareSubstring("ABD", "ABC", 0));
            Assert.False(VcfVariantUtilities.CompareSubstring("ABD", "ABC", 2));
        }

        [Fact]
        public void UnpackAlleles()
        {
            //two example vcf files that have been "crushed".
            var crushedVcf1 = Path.Combine(TestPaths.LocalTestDataDirectory, "VcfFileWriterTests_Crushed_Padded_expected.vcf");
            var crushedVcf2 = Path.Combine(TestPaths.LocalTestDataDirectory, "crushed.genome.vcf");

            var unpackedVariants1 = AlleleReader.GetAllVariantsInFile(crushedVcf1);
            var unpackedVariants2 = AlleleReader.GetAllVariantsInFile(crushedVcf2);

            Assert.Equal(8, unpackedVariants1.Count);//7 lines, but 8 alleles
            Assert.Equal(91, unpackedVariants2.Count);//90 lines, but 91 alleles

            var hetAlt1 = unpackedVariants1[5];
            var hetAlt2 = unpackedVariants2[3];
            var hetAlt1next = unpackedVariants1[6];
            var hetAlt2next = unpackedVariants2[4];

            //example one:
            //total depth = 5394, total variant count = 2387 + 2000 = 4387
            //so, ref counts ~1007.

            //example two:
            //total depth = 532, total variant count = 254 + 254 = 508
            //so, ref counts ~24.

            Assert.Equal(Genotype.HeterozygousAlt1Alt2, hetAlt1.Genotype);
            Assert.Equal(Genotype.HeterozygousAlt1Alt2, hetAlt2.Genotype);

            Assert.Equal(1007, hetAlt1.ReferenceSupport);
            Assert.Equal(2387, hetAlt1.AlleleSupport);
            Assert.Equal(0.4425, hetAlt1.Frequency, 4);

            Assert.Equal(24, hetAlt2.ReferenceSupport);
            Assert.Equal(254, hetAlt2.AlleleSupport);
            Assert.Equal(10, hetAlt1.ReferencePosition);
            Assert.Equal("AA", hetAlt1.ReferenceAllele);
            Assert.Equal("GA", hetAlt1.AlternateAllele);

            Assert.Equal(223906731, hetAlt2.ReferencePosition);

            Assert.Equal(1007, hetAlt1next.ReferenceSupport);
            Assert.Equal(2000, hetAlt1next.AlleleSupport);
            Assert.Equal("G", hetAlt1next.AlternateAllele);
            Assert.Equal(0.3708, hetAlt1next.Frequency, 4);

            Assert.Equal(24, hetAlt2next.ReferenceSupport);
            Assert.Equal(254, hetAlt2next.AlleleSupport);
      
            Assert.Equal(223906731, hetAlt2.ReferencePosition);
            Assert.Equal(10, hetAlt1next.ReferencePosition);
            Assert.Equal(223906731, hetAlt2next.ReferencePosition);

        }

        /// <summary>
        /// this whole test can be removed once vcf variant is gone
        /// </summary>
        [Fact]
        public void Convert()
        {
            var vcfVar = TestHelper.CreateDummyVariant("chr10", 123, "A", "C", 1000, 156);
            vcfVar.Genotypes[0]["GT"] = "0/1";
            var allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar);

            Assert.Equal(vcfVar.ReferenceName, allele.Chromosome);
            Assert.Equal(vcfVar.VariantAlleles[0], allele.AlternateAllele);
            Assert.Equal(vcfVar.ReferenceAllele, allele.ReferenceAllele);
            Assert.Equal(vcfVar.ReferencePosition, allele.ReferencePosition);
            Assert.Equal(new List<FilterType>() { }, allele.Filters);
            Assert.Equal(Genotype.HeterozygousAltRef, allele.Genotype);
            Assert.Equal(AlleleCategory.Snv, allele.Type);
            Assert.Equal(0.0100f, allele.FractionNoCalls);

            // no US filed
            Assert.Null(Record.Exception(() => VcfVariantUtilities.ConvertUnpackedVariant(vcfVar)));

            // exception case if US field doesn't match configuration 
            Assert.Throws<ArgumentOutOfRangeException>(() => VcfVariantUtilities.ConvertUnpackedVariant(vcfVar, true, true));

            // exception case if US field doesn't match configuration 
            Assert.Throws<ArgumentOutOfRangeException>(() => VcfVariantUtilities.ConvertUnpackedVariant(vcfVar, true, false));

            // exception case 
            vcfVar.Genotypes[0]["US"] = "1,2,3,4,5,6,7,8";
            Assert.Throws<ArgumentOutOfRangeException>(() => VcfVariantUtilities.ConvertUnpackedVariant(vcfVar, true, true));

           
            vcfVar.Genotypes[0]["US"] = "0,0,1,0,0,0,8,1,23,5,20,10";
            Assert.False(VcfVariantUtilities.ConvertUnpackedVariant(vcfVar, false, false).ReadCollapsedCountTotal.Any(x=>x!=0));

            // exception case
            vcfVar.Genotypes[0]["US"] = "0,0,1,0,0,0,8,1,23,5,20,10";
            Assert.Throws<ArgumentOutOfRangeException>(() => VcfVariantUtilities.ConvertUnpackedVariant(vcfVar, true, false));

            // test for collapsed reads against specs
            // When ReportRcCounts and ReportTsCounts are set as True, 
            // a US field will be outputted for each genotype as mutant support: 
            // duplex -stitched, duplex-nonstitched, simplex-forward-stitched, simplex-forward-nonstitched, simplex-reverse-stitched, simplex-reverse-nonstitched followed by total support: duplex-stitched, duplex-nonstitched, simplex-forward-stitched, simplex-forward-nonstitched, simplex-reverse-stitched, simplex-reverse-nonstitched.
            vcfVar.Genotypes[0]["US"] = "0,0,1,0,0,0,8,1,23,5,20,10";
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar, true, true);
            Assert.Equal(vcfVar.ReferenceName, allele.Chromosome);
            Assert.Equal(vcfVar.VariantAlleles[0], allele.AlternateAllele);
            Assert.Equal(vcfVar.ReferenceAllele, allele.ReferenceAllele);
            Assert.Equal(vcfVar.ReferencePosition, allele.ReferencePosition);
            Assert.Equal(new List<FilterType>(), allele.Filters);
            Assert.Equal(Genotype.HeterozygousAltRef, allele.Genotype);
            Assert.Equal(AlleleCategory.Snv, allele.Type);
            Assert.Equal(0.0100f, allele.FractionNoCalls);

            Assert.Equal(8, allele.ReadCollapsedCountTotal[(int)ReadCollapsedType.DuplexStitched]);
            Assert.Equal(1, allele.ReadCollapsedCountTotal[(int)ReadCollapsedType.DuplexNonStitched]);
            Assert.Equal(23, allele.ReadCollapsedCountTotal[(int)ReadCollapsedType.SimplexForwardStitched]);
            Assert.Equal(5, allele.ReadCollapsedCountTotal[(int)ReadCollapsedType.SimplexForwardNonStitched]);
            Assert.Equal(20, allele.ReadCollapsedCountTotal[(int)ReadCollapsedType.SimplexReverseStitched]);
            Assert.Equal(10, allele.ReadCollapsedCountTotal[(int)ReadCollapsedType.SimplexReverseNonStitched]);
            Assert.Equal(0, allele.ReadCollapsedCountsMut[(int)ReadCollapsedType.DuplexStitched]);
            Assert.Equal(0, allele.ReadCollapsedCountsMut[(int)ReadCollapsedType.DuplexNonStitched]);
            Assert.Equal(1, allele.ReadCollapsedCountsMut[(int)ReadCollapsedType.SimplexForwardStitched]);
            Assert.Equal(0, allele.ReadCollapsedCountsMut[(int)ReadCollapsedType.SimplexForwardNonStitched]);
            Assert.Equal(0, allele.ReadCollapsedCountsMut[(int)ReadCollapsedType.SimplexReverseStitched]);
            Assert.Equal(0, allele.ReadCollapsedCountsMut[(int)ReadCollapsedType.SimplexReverseNonStitched]);

            // When contain XV and XW tags, output read counts for duplex-stitched, duplex-nonstitched, simplex-stitched, and simplex-nonstitched.
            vcfVar.Genotypes[0]["US"] = "1,2,3,4,5,6,7,8";
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar, true, false);
            Assert.Equal(vcfVar.ReferenceName, allele.Chromosome);
            Assert.Equal(vcfVar.VariantAlleles[0], allele.AlternateAllele);
            Assert.Equal(vcfVar.ReferenceAllele, allele.ReferenceAllele);
            Assert.Equal(vcfVar.ReferencePosition, allele.ReferencePosition);
            Assert.Equal(new List<FilterType>(), allele.Filters);
            Assert.Equal(Genotype.HeterozygousAltRef, allele.Genotype);
            Assert.Equal(AlleleCategory.Snv, allele.Type);
            Assert.Equal(0.0100f, allele.FractionNoCalls);
            Assert.Equal(1, allele.ReadCollapsedCountsMut[(int)ReadCollapsedType.DuplexStitched]);
            Assert.Equal(2, allele.ReadCollapsedCountsMut[(int)ReadCollapsedType.DuplexNonStitched]);
            Assert.Equal(3, allele.ReadCollapsedCountsMut[(int)ReadCollapsedType.SimplexStitched]);
            Assert.Equal(4, allele.ReadCollapsedCountsMut[(int)ReadCollapsedType.SimplexNonStitched]);
            Assert.Equal(5, allele.ReadCollapsedCountTotal[(int)ReadCollapsedType.DuplexStitched]);
            Assert.Equal(6, allele.ReadCollapsedCountTotal[(int)ReadCollapsedType.DuplexNonStitched]);
            Assert.Equal(7, allele.ReadCollapsedCountTotal[(int)ReadCollapsedType.SimplexStitched]);
            Assert.Equal(8, allele.ReadCollapsedCountTotal[(int)ReadCollapsedType.SimplexNonStitched]);


            vcfVar.Genotypes[0]["GT"] = "./.";
            vcfVar.Filters = "R5x9";
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar);

            Assert.Equal(vcfVar.ReferenceName, allele.Chromosome);
            Assert.Equal(vcfVar.VariantAlleles[0], allele.AlternateAllele);
            Assert.Equal(vcfVar.ReferenceAllele, allele.ReferenceAllele);
            Assert.Equal(vcfVar.ReferencePosition, allele.ReferencePosition);
            Assert.Equal(new List<FilterType>() { FilterType.RMxN }, allele.Filters);
            Assert.Equal(Genotype.AltLikeNoCall, allele.Genotype);
            Assert.Equal(AlleleCategory.Snv, allele.Type);

            vcfVar.Genotypes[0]["GT"] = "1/2";
            vcfVar.Filters = "R5x9;SB";
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar);

            Assert.Equal(vcfVar.ReferenceName, allele.Chromosome);
            Assert.Equal(vcfVar.VariantAlleles[0], allele.AlternateAllele);
            Assert.Equal(vcfVar.ReferenceAllele, allele.ReferenceAllele);
            Assert.Equal(vcfVar.ReferencePosition, allele.ReferencePosition);
            Assert.Equal(new List<FilterType>() { FilterType.RMxN, FilterType.StrandBias }, allele.Filters);
            Assert.Equal(Genotype.HeterozygousAlt1Alt2, allele.Genotype);
            Assert.Equal(AlleleCategory.Snv, allele.Type);

            vcfVar.Genotypes[0]["GT"] = "1/1";
            vcfVar.Filters = "R8;q30";
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar);

            Assert.Equal(vcfVar.ReferenceName, allele.Chromosome);
            Assert.Equal(new List<FilterType>() { FilterType.IndelRepeatLength, FilterType.LowVariantQscore }, allele.Filters);
            Assert.Equal(Genotype.HomozygousAlt, allele.Genotype);
            Assert.Equal(AlleleCategory.Snv, allele.Type);

            vcfVar.Genotypes[0]["GT"] = "1/1";
            vcfVar.Filters = "lowvariantfreq;multiallelicsite";
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar);

            Assert.Equal(vcfVar.ReferenceName, allele.Chromosome);
            Assert.Equal(new List<FilterType>() { FilterType.LowVariantFrequency, FilterType.MultiAllelicSite }, allele.Filters);
            Assert.Equal(Genotype.HomozygousAlt, allele.Genotype);
            Assert.Equal(AlleleCategory.Snv, allele.Type);

            vcfVar.Filters = "lowsupport";
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar);

            Assert.Equal(vcfVar.ReferenceName, allele.Chromosome);
            Assert.Equal(new List<FilterType>() { FilterType.Unknown }, allele.Filters);
            Assert.Equal(Genotype.HomozygousAlt, allele.Genotype);
            Assert.Equal(AlleleCategory.Snv, allele.Type);

            vcfVar.Filters = "pass";
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar);

            Assert.Equal(vcfVar.ReferenceName, allele.Chromosome);
            Assert.Equal(new List<FilterType>() {  }, allele.Filters);
            Assert.Equal(Genotype.HomozygousAlt, allele.Genotype);
            Assert.Equal(AlleleCategory.Snv, allele.Type);

            // check handling of complex allele types (DO trimming, please)
            var originalPos = 10;
            vcfVar.VariantAlleles[0] = "CCGCA";
            vcfVar.ReferenceAllele = "CC";
            vcfVar.ReferencePosition = originalPos;
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar,false, false);
            Assert.Equal(originalPos+1, allele.ReferencePosition);
            Assert.Equal("CGCA", allele.AlternateAllele);
            Assert.Equal("C", allele.ReferenceAllele);

            // check handling of complex allele types (DO trimming, please)
            vcfVar.VariantAlleles[0] = "CGCAAA";
            vcfVar.ReferenceAllele = "CA";
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar, false, false);
            Assert.Equal(originalPos, allele.ReferencePosition);
            Assert.Equal("CGCAA", allele.AlternateAllele);
            Assert.Equal("C", allele.ReferenceAllele);

            // check handling of complex allele types (DO trimming, please)
            vcfVar.VariantAlleles[0] = "CCCGCAAA";
            vcfVar.ReferenceAllele = "CCCA";
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar, false, false);
            Assert.Equal(originalPos+2, allele.ReferencePosition);
            Assert.Equal("CGCAA", allele.AlternateAllele);
            Assert.Equal("C", allele.ReferenceAllele);


            // check handling of complex allele types (DO NOT trim)
            vcfVar.VariantAlleles[0] = "CCGCA";
            vcfVar.ReferenceAllele = "CC";
            vcfVar.ReferencePosition = originalPos;
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar, false, false, false);
            Assert.Equal(originalPos, allele.ReferencePosition);
            Assert.Equal("CCGCA", allele.AlternateAllele);
            Assert.Equal("CC", allele.ReferenceAllele);

            // check handling of complex allele types (DO NOT trim)
            vcfVar.VariantAlleles[0] = "CGCAAA";
            vcfVar.ReferenceAllele = "CA";
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar, false, false, false);
            Assert.Equal(originalPos, allele.ReferencePosition);
            Assert.Equal("CGCAAA", allele.AlternateAllele);
            Assert.Equal("CA", allele.ReferenceAllele);

            // check handling of complex allele types (DO NOT trim)
            vcfVar.VariantAlleles[0] = "CCCGCAAA";
            vcfVar.ReferenceAllele = "CAA";
            allele = VcfVariantUtilities.ConvertUnpackedVariant(vcfVar, false, false, false);
            Assert.Equal(originalPos, allele.ReferencePosition);
            Assert.Equal("CCCGCAAA", allele.AlternateAllele);
            Assert.Equal("CAA", allele.ReferenceAllele);

        }

        [Fact]
        public void TestGetNumTrailingAgreement()
        {
            Assert.Equal(0, VcfVariantUtilities.GetNumTrailingAgreement("ACGT", "CGTA"));
            Assert.Equal(1, VcfVariantUtilities.GetNumTrailingAgreement("AGT", "CGTAT"));
            Assert.Equal(2, VcfVariantUtilities.GetNumTrailingAgreement("ACGTGGG", "CGTAGG"));
            Assert.Equal(3, VcfVariantUtilities.GetNumTrailingAgreement("AAAA", "AAA"));
            Assert.Equal(3, VcfVariantUtilities.GetNumTrailingAgreement("AAA", "AAAA"));
            Assert.Equal(4, VcfVariantUtilities.GetNumTrailingAgreement("ACGT", "ACGT"));
            Assert.Equal(0, VcfVariantUtilities.GetNumTrailingAgreement("TAAAC", "CAAAAT"));

            Assert.Equal(0, VcfVariantUtilities.GetNumTrailingAgreement("TAATA", "TAATC"));
            Assert.Equal(2, VcfVariantUtilities.GetNumTrailingAgreement("TAATA", "TACTA"));
            Assert.Equal(3, VcfVariantUtilities.GetNumTrailingAgreement("TACGTG", "TATGTG"));
            Assert.Equal(3, VcfVariantUtilities.GetNumTrailingAgreement("TACGTG", "TAGTG"));
            Assert.Equal(3, VcfVariantUtilities.GetNumTrailingAgreement("TAGTG", "TACGTG"));
        }

        [Fact]
        public void TestGetNumPrecedingAgreement()
        {
            Assert.Equal(0, VcfVariantUtilities.GetNumPrecedingAgreement("ACGT", "CGTA"));
            Assert.Equal(1, VcfVariantUtilities.GetNumPrecedingAgreement("AT", "AGTAT"));
            Assert.Equal(2, VcfVariantUtilities.GetNumPrecedingAgreement("CGAATGGG", "CGTAGG"));
            Assert.Equal(3, VcfVariantUtilities.GetNumPrecedingAgreement("AAAA", "AAA"));
            Assert.Equal(3, VcfVariantUtilities.GetNumPrecedingAgreement("AAA", "AAAA"));
            Assert.Equal(4, VcfVariantUtilities.GetNumPrecedingAgreement("ACGT", "ACGT"));
            Assert.Equal(0, VcfVariantUtilities.GetNumPrecedingAgreement("TAAAC", "CAAAAT"));

            Assert.Equal(4, VcfVariantUtilities.GetNumPrecedingAgreement("TAATA", "TAATC"));
            Assert.Equal(2, VcfVariantUtilities.GetNumPrecedingAgreement("TAATA", "TACTA"));
            Assert.Equal(2, VcfVariantUtilities.GetNumPrecedingAgreement("TACGTG", "TATGTG"));
            Assert.Equal(2, VcfVariantUtilities.GetNumPrecedingAgreement("TACGTG", "TAGTG"));
            Assert.Equal(2, VcfVariantUtilities.GetNumPrecedingAgreement("TATGTG", "TACGTG"));
            Assert.Equal(2, VcfVariantUtilities.GetNumPrecedingAgreement("TAGTG", "TACGTG"));
        }

        [Fact]
        public void TestTrimUnsupportedAlleleType()
        {
            //we should trim from the back, so the position is conserved if possible.
            var allele = TestHelper.CreateDummyAllele("chr10", 123, "TAATA", "TAATAAATAAATA", 1000, 156);
            VcfVariantUtilities.TrimUnsupportedAlleleType(allele);
            Assert.Equal("T", allele.ReferenceAllele);
            Assert.Equal("TAATAAATA", allele.AlternateAllele);
            Assert.Equal(123, allele.ReferencePosition);


            allele = TestHelper.CreateDummyAllele("chr10", 123, "TAATA", "TAATA", 1000, 156);
            VcfVariantUtilities.TrimUnsupportedAlleleType(allele);
            Assert.Equal("T", allele.ReferenceAllele);
            Assert.Equal("T", allele.AlternateAllele);
            Assert.Equal(123, allele.ReferencePosition);

            allele = TestHelper.CreateDummyAllele("chr10", 123, "TAATAAATAAATA", "TAATA", 1000, 156);
            VcfVariantUtilities.TrimUnsupportedAlleleType(allele);
            Assert.Equal("TAATAAATA", allele.ReferenceAllele);
            Assert.Equal("T", allele.AlternateAllele);
            Assert.Equal(123, allele.ReferencePosition);


            allele = TestHelper.CreateDummyAllele("chr10", 123, "TACTAAATAAATA", "TAATA", 1000, 156);
            VcfVariantUtilities.TrimUnsupportedAlleleType(allele);
            // 123, "TACTAAATAAATA", "TAATA", 
            //-> 123, "TACTAAATA", "T",  (trim the back)
            //-> 123, "TACTAAATA", "T"       (trim the front - nothing)
            Assert.Equal("TACTAAATA", allele.ReferenceAllele);
            Assert.Equal("T", allele.AlternateAllele);
            Assert.Equal(123, allele.ReferencePosition);

            allele = TestHelper.CreateDummyAllele("chr10", 123, "TAATA", "TACTAAATAAATA", 1000, 156);
            VcfVariantUtilities.TrimUnsupportedAlleleType(allele);
            Assert.Equal("T", allele.ReferenceAllele);
            Assert.Equal("TACTAAATA", allele.AlternateAllele);
            Assert.Equal(123, allele.ReferencePosition);


            allele = TestHelper.CreateDummyAllele("chr10", 123, "TAATA", "TAATAAATAAATC", 1000, 156);
            VcfVariantUtilities.TrimUnsupportedAlleleType(allele);
            // 123, "TAATA",  "TAATAAATAAATC", 
            //-> 123,  "TAATA",  "TAATAAATAAATC",  (trim the back -nothing)
            //-> 123+4,  "(TAAT)A",  "(TAAT)AAATAAATC"      (trim the front - 4)
            Assert.Equal("A", allele.ReferenceAllele);
            Assert.Equal("AAATAAATC", allele.AlternateAllele);
            Assert.Equal(123 + 4, allele.ReferencePosition);

            //negative case, insertion, should leave it alone
            allele = TestHelper.CreateDummyAllele("chr10", 123, "T", "TAATAAATAAATC", 1000, 156);
            VcfVariantUtilities.TrimUnsupportedAlleleType(allele);
            Assert.Equal("T", allele.ReferenceAllele);
            Assert.Equal("TAATAAATAAATC", allele.AlternateAllele);
            Assert.Equal(123, allele.ReferencePosition);

            //negative case, deletion, should leave it alone
            allele = TestHelper.CreateDummyAllele("chr10", 123, "TAATAAATAAATC", "T", 1000, 156);
            VcfVariantUtilities.TrimUnsupportedAlleleType(allele);
            Assert.Equal("TAATAAATAAATC", allele.ReferenceAllele);
            Assert.Equal("T", allele.AlternateAllele);
            Assert.Equal(123, allele.ReferencePosition);

            //case that somehow got through all the other tests.. (found as a bug in the 5.2.6 RC testing)
            allele = TestHelper.CreateDummyAllele("chr10", 123, "CTGCCATACAGCTTCAACAACAACTT", "ATGCCATACAGCTTCAACAACAA", 1000, 156);
            VcfVariantUtilities.TrimUnsupportedAlleleType(allele);
            Assert.Equal("CTGCCATACAGCTTCAACAACAACTT", allele.ReferenceAllele);
            Assert.Equal("ATGCCATACAGCTTCAACAACAA", allele.AlternateAllele);
            Assert.Equal(123, allele.ReferencePosition);
            
            allele = TestHelper.CreateDummyAllele("chr10", 123, "ATGCCATACAGCTTCAACAACAA", "CTGCCATACAGCTTCAACAACAACTT", 1000, 156);
            VcfVariantUtilities.TrimUnsupportedAlleleType(allele);
            Assert.Equal("ATGCCATACAGCTTCAACAACAA", allele.ReferenceAllele);
            Assert.Equal("CTGCCATACAGCTTCAACAACAACTT", allele.AlternateAllele);
            Assert.Equal(123, allele.ReferencePosition);
            
            allele = TestHelper.CreateDummyAllele("chr10", 123, "A", "C", 1000, 156);
            VcfVariantUtilities.TrimUnsupportedAlleleType(allele);
            Assert.Equal("A", allele.ReferenceAllele);
            Assert.Equal("C", allele.AlternateAllele);
            Assert.Equal(123, allele.ReferencePosition);
            
            allele = TestHelper.CreateDummyAllele("chr10", 123, "A", "A", 1000, 156);
            VcfVariantUtilities.TrimUnsupportedAlleleType(allele);
            Assert.Equal("A", allele.ReferenceAllele);
            Assert.Equal("A", allele.AlternateAllele);
            Assert.Equal(123, allele.ReferencePosition);

        }
    }
}

