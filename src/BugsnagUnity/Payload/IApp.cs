namespace BugsnagUnity.Payload
{
    public interface IApp
    {
        string BinaryArch { get; set; }
        string BuildUuid { get; set; }
        string BundleVersion { get; set; }
        string CodeBundleId { get; set; }
        string DsymUuid { get; set; }
        string Id { get; set; }
        string ReleaseStage { get; set; }
        string Type { get; set; }
        string Version { get; set; }
        int? VersionCode { get; set; }
    }
}