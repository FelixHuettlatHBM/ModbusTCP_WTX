using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace HBM.WT.API.COMMON
{
    public class NetConnectionSecurity
    {
        /// <summary>
        /// RemoteCertificationCheck:
        /// Callback-Method wich is called from SslStream. Is a customized implementation of a certification-check.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        public static bool RemoteCertificationCheck(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            try {
                X509Certificate2 clientCertificate = new X509Certificate2("ssh_server_cert.pem");
                SslStream sslStream = (sender as SslStream);

                if (sslPolicyErrors == SslPolicyErrors.None || sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors) {
                    foreach (X509ChainElement item in chain.ChainElements) {
                        item.Certificate.Export(X509ContentType.Cert);
                        //
                        // If one of the included status-flags is not posiv then the cerficate-check
                        // failed. Except the "untrusted root" because it is a self-signed certificate
                        //
                        foreach (X509ChainStatus status in item.ChainElementStatus) {
                            if (status.Status != X509ChainStatusFlags.NoError
                                && status.Status != X509ChainStatusFlags.UntrustedRoot
                                 && status.Status != X509ChainStatusFlags.NotTimeValid) {

                                return false;
                            }
                        }
                        //
                        // compare the certificate in the chain-collection. If on of the certificate at
                        // the path to root equal, are the check ist positive
                        //
                        if (clientCertificate.Equals(item.Certificate)) {
                            return true;
                        }
                    }
                }
                // TODO: to reactivate the hostename-check returning false.
                return true;
            }
            catch (Exception) {
                // If thrown any exception then is the certification-check failed
                return false;
            }
        }
    }
}
