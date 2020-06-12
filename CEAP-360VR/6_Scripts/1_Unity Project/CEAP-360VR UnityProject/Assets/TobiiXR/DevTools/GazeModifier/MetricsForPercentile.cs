using System.Collections.Generic;
using System.Linq;
using Tobii.XR.GazeModifier;
using UnityEngine;

namespace Tobii.XR
{
    public class MetricsForPercentile : IMetricsForPercentile
    {
        private readonly ILerpValue _accuracyForPercentile = new GetValueForAngle();
        private readonly ILerpValue _precisionForPercentile = new GetValueForAngle();
        private readonly ILerpValue _trackabilityForPercentile = new GetValueForAngle();
        private int _currentPercentile = -1;
        private readonly IList<PercentileData> _percentileData;

        public float Accuracy(int percentile, float angle)
        {
            CheckCurrentPercentile(percentile);
            return _accuracyForPercentile.Evaluate(angle);
        }

        public float Precision(int percentile, float angle)
        {
            CheckCurrentPercentile(percentile);
            return _precisionForPercentile.Evaluate(angle);
        }

        public float Trackability(int percentile, float angle)
        {
            CheckCurrentPercentile(percentile);
            return _trackabilityForPercentile.Evaluate(angle);
        }

        private void CheckCurrentPercentile(int percentile)
        {
            if (_currentPercentile != percentile)
            {
                _currentPercentile = percentile;
                _accuracyForPercentile.SetValues(_percentileData.Where(p => p.Percentile == _currentPercentile).Select(p => new Vector2(p.Angle, p.Accuracy)));
                _precisionForPercentile.SetValues(_percentileData.Where(p => p.Percentile == _currentPercentile).Select(p => new Vector2(p.Angle, p.Precision)));
                _trackabilityForPercentile.SetValues(_percentileData.Where(p => p.Percentile == _currentPercentile).Select(p => new Vector2(p.Angle, p.Trackability)));
            }
        }

        public MetricsForPercentile(IPercentileRepository percentileRepository)
        {
            _percentileData = percentileRepository.LoadAll();
        }
    }

    public interface IMetricsForPercentile
    {
        float Accuracy(int percentile, float angle);
        float Precision(int percentile, float angle);
        float Trackability(int percentile, float angle);
    }
}