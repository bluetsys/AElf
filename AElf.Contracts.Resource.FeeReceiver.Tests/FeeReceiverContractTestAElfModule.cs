using AElf.Contracts.TestBase;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace AElf.Contracts.Resource.FeeReceiver
{
    [DependsOn(typeof(ContractTestAElfModule))]
    public class FeeReceiverContractTestAElfModule : ContractTestAElfModule
    {
    }
}