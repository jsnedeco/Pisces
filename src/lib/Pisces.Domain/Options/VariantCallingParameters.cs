﻿using System;
using Pisces.Domain.Models.Alleles;
using Pisces.Domain.Types;

namespace Pisces.Domain.Options
{

    public class DiploidThresholdingParameters
    {
        public float MinorVF { get; set; } = 0.20f;
        public float MajorVF { get; set; } = 0.70f;
        public float SumVFforMultiAllelicSite { get; set; } = 0.80f;

        //not too safe, but dev use only.
        public DiploidThresholdingParameters(float[] parameters)
        {
            MinorVF = parameters[0];
            MajorVF = parameters[1];
            SumVFforMultiAllelicSite = parameters[2];
        }

        public override string ToString()
        {
            return (string.Format("{0},{1},{2}", MinorVF, MajorVF, SumVFforMultiAllelicSite));
        }
    }

    public class AdaptiveGenotypingParameters
    {

        // for when we fall back to thresholding alg
        public float SumVFforMultiAllelicSite { get; set; } = 0.80F;
        public int MaxGenotypePosteriors { get; set; } = 3000;

        // Each model is a 3 element double array.
        public double[] SnvModel { get; set; } = new double[] { 0.037, 0.439, 0.976 };
        public double[] IndelModel { get; set; } = new double[] { 0.037, 0.443, 0.905 };
        public double[] SnvPrior { get; set; } = new double[] { 0.755, 0.154, 0.0919 };
        public double[] IndelPrior { get; set; } = new double[] { 0.962, 0.0266, 0.0114 };

        public AdaptiveGenotypingParameters() { }

        public (double[] model, double[] priors) GetModelsAndPriors(BaseAllele allele)
        {
            if (allele.Type == AlleleCategory.Snv ||
                allele.Type == AlleleCategory.Reference ||
                allele.Type == AlleleCategory.Mnv)
                return (SnvModel, SnvPrior);
            else if (allele.Type == AlleleCategory.Deletion ||
                allele.Type == AlleleCategory.Insertion)
                return (IndelModel, IndelPrior);

            throw new ArgumentException("Allele category unsupported for diploid adaptive model.");
        }
    }

    public class VariantCallingParameters
    {
        public float MinimumFrequency = 0.01f;
        public float MinimumFrequencyFilter = -1; //if unset, will raise to MinimumFrequency 
        public float TargetLODFrequency = -1; //if unset, will raise to TargetLODFrequency

        public int MaximumVariantQScore = 100;
        public int MinimumVariantQScore = 20;
        public int MinimumVariantQScoreFilter = 30;

        public int MaximumGenotypeQScore = 100;
        public int MinimumGenotypeQScore = 0;
        public int? LowGenotypeQualityFilter;

        public int MinimumCoverage = 10;
        public int? LowDepthFilter;

        public int? IndelRepeatFilter;

        public int? RMxNFilterMaxLengthRepeat = 5;
        public int? RMxNFilterMinRepetitions = 9;
        public float RMxNFilterFrequencyLimit = 0.35f; //
        //originally set to 20%, recommended by Kristina K after empirical testing, 2017 
        //adjusted to 35% following PICS-967 study, 2018

        public PloidyModel PloidyModel = PloidyModel.Somatic;
        public AdaptiveGenotypingParameters AdaptiveGenotypingParameters = new AdaptiveGenotypingParameters();
        public DiploidThresholdingParameters DiploidSNVThresholdingParameters = new DiploidThresholdingParameters(new float[] { 0.20F, 0.70F, 0.80F });
        public DiploidThresholdingParameters DiploidINDELThresholdingParameters = new DiploidThresholdingParameters(new float[] { 0.20F, 0.70F, 0.80F });
        //originally both set to {0.20F, 0.70F, 0.80F}, recommended by Dorothea A after empirical testing, 2017 
        //re-assessed for AmpliSeq to {0.20F, 0.90F, 0.80F} for SNPs, and then returned to {0.20F, 0.70F, 0.80F} as still being the best general settings.

        public bool? IsMale;

        public int ForcedNoiseLevel = -1;
        public int NoiseLevelUsedForQScoring = 20;
        public NoiseModel NoiseModel = NoiseModel.Flat;

        public float StrandBiasAcceptanceCriteria = 0.5f;
        public StrandBiasModel StrandBiasModel = StrandBiasModel.Extended;  //maybe should add "none" for scylla
        public bool FilterOutVariantsPresentOnlyOneStrand = false;

        public float NoCallFilterThreshold = 0.6f;

        //tjd+
        //public float? AmpliconBiasFilterThreshold = 0.01f; <- turing this off by default for 5.2.11 release.
        // but I'd like to have it on as soon as we have it more well tested.
        //The concern here is just with performance/speed & objectionable command line constipation,
        //Not really with the new feature, per se
        //tjd -
        public float? AmpliconBiasFilterThreshold = null;

        public void SetDerivedParameters(BamFilterParameters bamFilterParameters)
        {

            NoiseLevelUsedForQScoring = GetNoiseLevelUsedForQScoring(bamFilterParameters);
        }

        public int GetNoiseLevelUsedForQScoring(BamFilterParameters bamFilterParameters)
        {
            return ForcedNoiseLevel == -1 ? bamFilterParameters.MinimumBaseCallQuality : ForcedNoiseLevel;
        }

        public void Validate()
        {
            ValidationHelper.VerifyRange(MinimumVariantQScore, 0, int.MaxValue, "MinimumVariantQscore");
            ValidationHelper.VerifyRange(MaximumVariantQScore, 0, int.MaxValue, "MaximumVariantQScore");
            if (MaximumVariantQScore < MinimumVariantQScore)
                throw new ArgumentException("MinimumVariantQScore must be less than or equal to MaximumVariantQScore.");

            ValidationHelper.VerifyRange(MinimumFrequency, 0f, 1f, "MinimumFrequency");
            ValidationHelper.VerifyRange(MinimumVariantQScoreFilter, MinimumVariantQScore, MaximumVariantQScore, "FilteredVariantQScore");

            if (LowGenotypeQualityFilter != null)
                ValidationHelper.VerifyRange((float)LowGenotypeQualityFilter, 0, int.MaxValue, "FilteredLowGenomeQuality");
            if (IndelRepeatFilter != null)
                ValidationHelper.VerifyRange((int)IndelRepeatFilter, 0, 10, "FilteredIndelRepeats");
            if (LowDepthFilter != null)
                ValidationHelper.VerifyRange((int)LowDepthFilter, MinimumCoverage, int.MaxValue, "FilteredLowDepth");

            if ((LowDepthFilter == null) || (LowDepthFilter < MinimumCoverage))
            {
                Console.WriteLine("Options helper: Setting LowDepthFilter = MinimumCoverage");
                LowDepthFilter = MinimumCoverage;
            }


            if (MinimumFrequencyFilter < MinimumFrequency)
            {
                Console.WriteLine("Options helper: Setting MinimumFrequencyFilter to MinimumFrequency");
                MinimumFrequencyFilter = MinimumFrequency;
            }
            ValidationHelper.VerifyRange(MinimumFrequencyFilter, 0, 1f, "FilteredVariantFrequency");


            if (TargetLODFrequency < MinimumFrequencyFilter)
            {
                Console.WriteLine("Options helper: Setting TargetLODFrequency to MinimumFrequencyFilter");
                TargetLODFrequency = MinimumFrequencyFilter;
            }
            ValidationHelper.VerifyRange(TargetLODFrequency, 0, 1f, "TargetLODFrequency");


            if (ForcedNoiseLevel != -1)
                ValidationHelper.VerifyRange(ForcedNoiseLevel, 0, int.MaxValue, "AppliedNoiseLevel");

            ValidationHelper.VerifyRange(StrandBiasAcceptanceCriteria, 0f, int.MaxValue, "Strand bias cutoff");


            if (RMxNFilterMaxLengthRepeat != null || RMxNFilterMinRepetitions != null)
            {
                if (RMxNFilterMaxLengthRepeat == null || RMxNFilterMinRepetitions == null)
                {
                    throw new ArgumentException(string.Format("If specifying RMxN filter thresholds, you must supply both RMxNFilterMaxLengthRepeat and RMxNFilterMinRepetitions."));
                }
                ValidationHelper.VerifyRange((int)RMxNFilterMaxLengthRepeat, 0, 100, "RMxNFilterMaxLengthRepeat");
                ValidationHelper.VerifyRange((int)RMxNFilterMinRepetitions, 0, 100, "RMxNFilterMinRepetitions");
            }

            ValidationHelper.VerifyRange(NoCallFilterThreshold, 0f, 1f, "NoCallFilterThreshold");

        }


    }


    public class DerivedParameters
    {

        public static int GetNoiseLevelUsedForQScoring(
            VariantCallingParameters varCallingParameters, BamFilterParameters bamFilterParameters)
        {
            return varCallingParameters.ForcedNoiseLevel == -1 ? bamFilterParameters.MinimumBaseCallQuality : varCallingParameters.ForcedNoiseLevel;
        }

    }

}
