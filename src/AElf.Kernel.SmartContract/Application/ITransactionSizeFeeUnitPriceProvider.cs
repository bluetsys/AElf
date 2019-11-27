using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Sdk.CSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;

namespace AElf.Kernel.SmartContract.Application
{
    public interface ITransactionSizeFeeUnitPriceProvider
    {
        void SetUnitPrice(long unitPrice);
        Task<long> GetUnitPriceAsync();
    }

    /// <summary>
    /// For testing.
    /// </summary>
    public class DefaultTransactionSizeFeeUnitPriceProvider : ITransactionSizeFeeUnitPriceProvider
    {
        private long _unitPrice;

        public ILogger<DefaultTransactionSizeFeeUnitPriceProvider> Logger { get; set; }

        public DefaultTransactionSizeFeeUnitPriceProvider()
        {
            Logger = new NullLogger<DefaultTransactionSizeFeeUnitPriceProvider>();
        }

        public void SetUnitPrice(long unitPrice)
        {
            Logger.LogError("Set tx size unit price wrongly.");
            _unitPrice = unitPrice;
        }

        public Task<long> GetUnitPriceAsync()
        {
            Logger.LogError("Get tx size unit price wrongly.");
            return Task.FromResult(_unitPrice);
        }
    }

    public enum FeeType
    {
        Tx = 0,
        Cpu,
        Sto,
        Ram,
        Net
    }
    public interface ICalculateFeeService : ISingletonDependency
    {
        long GetFee(FeeType feeType, int cost);
    }

    public class CalculateFeeService : ICalculateFeeService
    {
        private readonly ICalStradegyProvider _calStradegyProvider;

        public CalculateFeeService(ICalStradegyProvider calStradegyProvider)
        {
            _calStradegyProvider = calStradegyProvider;
        }
        public long GetFee(FeeType feeType, int cost)
        {
            return _calStradegyProvider.GetCalculator(feeType).GetCost(cost);
        }
    }

    public interface ICalStradegyProvider : ISingletonDependency
    {
        ICalCostStrategy GetCalculator(FeeType feeType);
    }

    public class CalStradegyProvider : ICalStradegyProvider
    {
        private Dictionary<FeeType, ICalCostStrategy> CalDic { get; set; }
        public CalStradegyProvider()
        {
            CalDic = new Dictionary<FeeType, ICalCostStrategy>
            {
                [FeeType.Cpu] = new CpuCalCostStrategy(),
                [FeeType.Sto] = new StoCalCostStrategy(),
                [FeeType.Net] = new NetCalCostStrategy(),
                [FeeType.Tx] = new TxCalCostStrategy()
            };
        }

        public ICalCostStrategy GetCalculator(FeeType feeType)
        {
            CalDic.TryGetValue(feeType, out var cal);
            return cal;
        }
    }
    public interface ICalCostStrategy
    {
        long GetCost(int cost);
    }
    public class CpuCalCostStrategy : ICalCostStrategy
    {
        public  ICalAlgorithm CalAlgorithm { get; set; }

        public CpuCalCostStrategy()
        {
            CalAlgorithm = new CalAlgorithm().Add(10, x => new LinerCalService
            {
                Numerator = 1,
                Denominator = 8,
                ConstantValue = 10000
            }.GetCost(x)).Add(100, x => new LinerCalService
            {
                Numerator = 1,
                Denominator = 4
            }.GetCost(x)).Add(int.MaxValue, x => new PowCalService
            {
                Power = 2,
                ChangeSpanBase = 4, // scale  x axis         
                Weight = 333, // unit weight,  means  (10 cpu count = 333 weight) 
                WeightBase = 40,
                Numerator = 1,
                Denominator = 4,
                Decimal = 100000000L // 1 token = 100000000
            }.GetCost(x)).Prepare();
        }
        public long GetCost(int cost)
        {
            return CalAlgorithm.Calculate(cost);
        }
    }
    public class StoCalCostStrategy : ICalCostStrategy
    {
        public  ICalAlgorithm CalAlgorithm { get; set; }

        public StoCalCostStrategy()
        {
            CalAlgorithm = new CalAlgorithm().Add(10, x => new LinerCalService
            {
                Numerator = 1,
                Denominator = 8,
                ConstantValue = 10000
            }.GetCost(x)).Add(100, x => new LinerCalService
            {
                Numerator = 1,
                Denominator = 4
            }.GetCost(x)).Add(int.MaxValue, x => new PowCalService
            {
                Power = 2,
                ChangeSpanBase = 2,
                Weight = 333,
                Numerator = 1,
                Denominator = 4,
                WeightBase = 40,
            }.GetCost(x)).Prepare();
        }
        public long GetCost(int cost)
        {
            return CalAlgorithm.Calculate(cost);
        }
    }
    public class RamCalCostStrategy : ICalCostStrategy
    {
        public  ICalAlgorithm CalCostService { get; set; }
        public long GetCost(int cost)
        {
            return CalCostService.Calculate(cost);
        }
    }
    public class NetCalCostStrategy : ICalCostStrategy
    {
        public  ICalAlgorithm CalAlgorithm { get; set; }

        public NetCalCostStrategy()
        {
            CalAlgorithm = new CalAlgorithm().Add(1000000, x => new LinerCalService
            {
                Numerator = 1,
                Denominator = 32,
                ConstantValue = 10000
            }.GetCost(x)).Add(int.MaxValue, x => new PowCalService
            {
                Power = 2,
                ChangeSpanBase = 100,
                Weight = 333,
                WeightBase = 500,
                Numerator = 1,
                Denominator = 32,
                Decimal = 100000000L
            }.GetCost(x)).Prepare();
        }
        public long GetCost(int cost)
        {
            return CalAlgorithm.Calculate(cost);
        }
    }
    public class TxCalCostStrategy : ICalCostStrategy
    {
        public  ICalAlgorithm CalAlgorithm { get; set; }

        public TxCalCostStrategy()
        {
            CalAlgorithm = new CalAlgorithm().Add(1000000, x => new LinerCalService
            {
                Numerator = 1,
                Denominator = 16 * 50,
                ConstantValue = 10000
            }.GetCost(x)).Add(int.MaxValue, x => new PowCalService
            {
                Power = 2,
                ChangeSpanBase = 100,
                Weight = 1,
                WeightBase = 1,
                Numerator = 1,
                Denominator = 16 * 50,
                Decimal = 100000000L
            }.GetCost(x)).Prepare();
        }
        public long GetCost(int cost)
        {
            return CalAlgorithm.Calculate(cost);
        }
    }

    public interface ICalAlgorithm
    {
        Dictionary<int, Func<int, long>> PieceWise { get; set; }
        long Calculate(int count);
        ICalAlgorithm Add(int limit, Func<int, long> func);
        ICalAlgorithm Prepare();
        void Delete();
        void Update();
    }

    public class CalAlgorithm : ICalAlgorithm
    {
        public ILogger<CalAlgorithm> Logger { get; set; }

        public CalAlgorithm()
        {
            Logger = new NullLogger<CalAlgorithm>();
        }

        public Dictionary<int, Func<int, long>> PieceWise { get; set; } = new Dictionary<int, Func<int, long>>();

        public ICalAlgorithm Add(int limit, Func<int, long> func)
        {
            // to do
            PieceWise[limit] = func;
            return this;
        }

        public ICalAlgorithm Prepare()
        {
            if (!PieceWise.Any() || PieceWise.Any(x => x.Key <= 0))
            {
                Logger.LogError("piece key wrong");
            }

            PieceWise = PieceWise.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            return this;
        }

        public long Calculate(int count)
        {
            long totalCost = 0;
            int prePieceKey = 0;
            foreach (var piece in PieceWise)
            {
                if (count < piece.Key)
                {
                    totalCost = piece.Value.Invoke(count.Sub(prePieceKey)).Add(totalCost);
                    break;
                }

                var span = piece.Key.Sub(prePieceKey);
                totalCost = piece.Value.Invoke(span).Add(totalCost);
                prePieceKey = piece.Key;
                if (count == piece.Key)
                    break;
            }

            return totalCost;
        }

        public void Delete()
        {
            
        }
        public void Update()
        {
            
        }
    }
    #region cal imp     

    public interface ICalService
    {
        long GetCost(int initValue);
        long Decimal { get; set; }
    }

    public class LnCalService : ICalService
    {
        public int ChangeSpanBase { get; set; }
        public long Weight { get; set; }
        public int WeightBase { get; set; }
        public long Decimal { get; set; } = 100000000L;

        public long GetCost(int cost)
        {
            int diff = cost + 1;
            double weightChange = (double) diff / ChangeSpanBase;
            double unitValue = (double) Weight / WeightBase;
            if (weightChange <= 1)
                return 0;
            return Decimal.Mul((long) (weightChange * unitValue * Math.Log(weightChange, Math.E)));
        }
    }

    public class PowCalService : ICalService
    {
        public double Power { get; set; }
        public int ChangeSpanBase { get; set; }
        public long Weight { get; set; }
        public int WeightBase { get; set; }
        public long Decimal { get; set; } = 100000000L;
        public int Numerator { get; set; }
        public int Denominator { get; set; } = 1;

        public long GetCost(int cost)
        {
            return ((long) (Math.Pow((double) cost / ChangeSpanBase, Power) * Decimal)).Mul(Weight).Div(WeightBase)
                .Add(Decimal.Mul(Numerator).Div(Denominator).Mul(cost));
        }
    }

    public class BancorCalService : ICalService
    {
        public decimal ResourceWeight { get; set; }
        public decimal TokenWeight { get; set; }
        public long ResourceConnectorBalance { get; set; }
        public long TokenConnectorBalance { get; set; }
        public long Decimal { get; set; } = 100000000L;

        public long GetCost(int cost)
        {
            throw new NotImplementedException();
        }
    }

    public class ConstCalService : ICalService
    {
        public long Decimal { get; set; } = 100000000L;
        public int ConstantValue { get; set; }

        public long GetCost(int cost)
        {
            return Decimal.Mul(ConstantValue);
        }
    }

    public class LinerCalService : ICalService
    {
        public int Numerator { get; set; }
        public int Denominator { get; set; } = 1;
        public int ConstantValue { get; set; }
        public long Decimal { get; set; } = 100000000L;

        public long GetCost(int cost)
        {
            return Decimal.Mul(cost).Mul(Numerator).Div(Denominator).Add(ConstantValue);
        }
    }

    #endregion
}