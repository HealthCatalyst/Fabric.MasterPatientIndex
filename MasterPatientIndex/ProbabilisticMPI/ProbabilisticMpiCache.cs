namespace MasterPatientIndex.ProbabilisticMPI
{
    public class ProbabilisticMpiCache : ProbabilisticMpiCacheBase
    {
        public ProbabilisticMpiCache(IPatientStore store, IMpiConfiguration configuration) : base(store, configuration)
        {
        }
    }
}