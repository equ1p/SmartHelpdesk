using Raven.Client.Documents;
using SmartHelpdesk.Models;

namespace SmartHelpdesk.Services
{
    
    public class DatabaseSeeder
    {
        private readonly IDocumentStore _store;

        public DatabaseSeeder(IDocumentStore store)
        {
            _store = store;
        }

        public async Task SeedAsync()
        {
            using var session = _store.OpenAsyncSession();

            // Check if already seeded
            var hasUsers = await session.Query<User>().AnyAsync();
            if (hasUsers) return;

            // ── Users ─────────────────────────────────────────────
            var client1 = new User { Name = "John Smith", Email = "john.smith@example.com", Role = UserRole.Client };
            var client2 = new User { Name = "Alice Johnson", Email = "alice.johnson@example.com", Role = UserRole.Client };
            var client3 = new User { Name = "Bob Williams", Email = "bob.williams@example.com", Role = UserRole.Client };
            var operator1 = new User { Name = "Sarah Davis", Email = "sarah.davis@helpdesk.com", Role = UserRole.Operator };
            var operator2 = new User { Name = "Mike Brown", Email = "mike.brown@helpdesk.com", Role = UserRole.Operator };

            await session.StoreAsync(client1);
            await session.StoreAsync(client2);
            await session.StoreAsync(client3);
            await session.StoreAsync(operator1);
            await session.StoreAsync(operator2);

            // Save to generate IDs (RavenDB HiLo algorithm assigns IDs client-side)
            await session.SaveChangesAsync();

            // ── Tickets ───────────────────────────────────────────
            // Open a new session (previous one already saved)
            using var session2 = _store.OpenAsyncSession();

            var tickets = new List<Ticket>
            {
                // Open tickets (some with SLA risk)
                new Ticket
                {
                    Title = "Cannot login to the system",
                    Description = "I get an 'Invalid credentials' error when trying to log in with my email.",
                    Status = TicketStatus.Open,
                    Priority = TicketPriority.High,
                    ClientId = client1.Id!,
                    CreatedAt = DateTime.UtcNow.AddHours(-30), // Over 25h SLA → violated
                    SlaHours = 25
                },
                new Ticket
                {
                    Title = "Printer not working on 3rd floor",
                    Description = "The HP LaserJet on the 3rd floor shows 'Offline' status.",
                    Status = TicketStatus.Open,
                    Priority = TicketPriority.Medium,
                    ClientId = client2.Id!,
                    CreatedAt = DateTime.UtcNow.AddHours(-5),
                    SlaHours = 25
                },
                new Ticket
                {
                    Title = "VPN disconnects every 10 minutes",
                    Description = "After the latest update, VPN connection drops frequently.",
                    Status = TicketStatus.Open,
                    Priority = TicketPriority.Critical,
                    ClientId = client3.Id!,
                    CreatedAt = DateTime.UtcNow.AddHours(-50), // Over 25h SLA → violated
                    SlaHours = 25
                },

                // In Progress tickets
                new Ticket
                {
                    Title = "Email not syncing on mobile",
                    Description = "Outlook app on iPhone stopped syncing emails 2 days ago.",
                    Status = TicketStatus.InProgress,
                    Priority = TicketPriority.Medium,
                    ClientId = client1.Id!,
                    OperatorId = operator1.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-20),
                    SlaHours = 25
                },
                new Ticket
                {
                    Title = "Request for new software license",
                    Description = "Need a license for Adobe Creative Suite for the design department.",
                    Status = TicketStatus.InProgress,
                    Priority = TicketPriority.Low,
                    ClientId = client2.Id!,
                    OperatorId = operator2.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-48), // Over 25h SLA → violated
                    SlaHours = 25
                },

                // Closed tickets (needed for Map-Reduce analytics)
                new Ticket
                {
                    Title = "Password reset request",
                    Description = "User forgot their password and needs a reset link.",
                    Status = TicketStatus.Closed,
                    Priority = TicketPriority.Low,
                    ClientId = client1.Id!,
                    OperatorId = operator1.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    ClosedAt = DateTime.UtcNow.AddDays(-5).AddHours(2),
                    SlaHours = 25
                },
                new Ticket
                {
                    Title = "Monitor flickering",
                    Description = "The Dell monitor on desk 42 is flickering intermittently.",
                    Status = TicketStatus.Closed,
                    Priority = TicketPriority.Medium,
                    ClientId = client3.Id!,
                    OperatorId = operator1.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    ClosedAt = DateTime.UtcNow.AddDays(-6).AddHours(18),
                    SlaHours = 25
                },
                new Ticket
                {
                    Title = "Install Microsoft Teams",
                    Description = "New employee needs Microsoft Teams installed on their workstation.",
                    Status = TicketStatus.Closed,
                    Priority = TicketPriority.Low,
                    ClientId = client2.Id!,
                    OperatorId = operator2.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    ClosedAt = DateTime.UtcNow.AddDays(-3).AddHours(1),
                    SlaHours = 25
                },
                new Ticket
                {
                    Title = "Slow internet in conference room B",
                    Description = "WiFi speed in conference room B drops below 5 Mbps during meetings.",
                    Status = TicketStatus.Closed,
                    Priority = TicketPriority.High,
                    ClientId = client1.Id!,
                    OperatorId = operator1.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    ClosedAt = DateTime.UtcNow.AddDays(-8),
                    SlaHours = 25
                },
                new Ticket
                {
                    Title = "Update antivirus definitions",
                    Description = "Antivirus on all accounting department PCs needs manual update.",
                    Status = TicketStatus.Closed,
                    Priority = TicketPriority.Medium,
                    ClientId = client3.Id!,
                    OperatorId = operator2.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    ClosedAt = DateTime.UtcNow.AddDays(-3).AddHours(8),
                    SlaHours = 25
                },

                // Old stale tickets (for PatchByQuery testing)
                new Ticket
                {
                    Title = "Old: Replace keyboard on desk 15",
                    Description = "The spacebar key is sticking on the keyboard at desk 15.",
                    Status = TicketStatus.Open,
                    Priority = TicketPriority.Low,
                    ClientId = client2.Id!,
                    CreatedAt = DateTime.UtcNow.AddDays(-45), // Stale ticket for PatchByQuery test
                    SlaHours = 25
                },
                new Ticket
                {
                    Title = "Old: Update training room projector",
                    Description = "The projector firmware in the training room needs updating.",
                    Status = TicketStatus.InProgress,
                    Priority = TicketPriority.Low,
                    ClientId = client1.Id!,
                    OperatorId = operator2.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-60), // Stale ticket for PatchByQuery test
                    SlaHours = 25
                }
            };

            foreach (var ticket in tickets)
                await session2.StoreAsync(ticket);

            await session2.SaveChangesAsync();

            // ── Articles ──────────────────────────────────────────
            using var session3 = _store.OpenAsyncSession();

            var articles = new List<Article>
            {
                new Article
                {
                    Title = "How to Reset Your Password",
                    Content = "If you forgot your password, follow these steps: 1. Go to the login page. 2. Click 'Forgot Password'. 3. Enter your email address. 4. Check your inbox for a reset link. 5. Click the link and create a new password. Make sure your password is at least 8 characters long and includes a mix of letters, numbers, and special characters.",
                    Tags = new List<string> { "password", "account", "security", "login" },
                    VectorEmbedding = new float[] { 0.12f, 0.85f, 0.33f, 0.67f, 0.21f, 0.94f, 0.45f, 0.78f },
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Article
                {
                    Title = "VPN Connection Troubleshooting Guide",
                    Content = "If your VPN keeps disconnecting, try these solutions: 1. Restart the VPN client. 2. Check your internet connection. 3. Switch to a different VPN server. 4. Update the VPN client to the latest version. 5. Disable your firewall temporarily to test. 6. Contact IT support if the problem persists. Common error codes: 800 (server unreachable), 619 (connection dropped), 691 (authentication failed).",
                    Tags = new List<string> { "vpn", "network", "connectivity", "troubleshooting" },
                    VectorEmbedding = new float[] { 0.88f, 0.15f, 0.72f, 0.39f, 0.56f, 0.08f, 0.91f, 0.23f },
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new Article
                {
                    Title = "Email Configuration for Outlook",
                    Content = "To set up your corporate email in Microsoft Outlook: 1. Open Outlook and go to File > Add Account. 2. Enter your work email address. 3. Outlook will auto-detect settings. 4. If auto-detect fails, use manual setup: IMAP server: mail.company.com, Port: 993, SSL: Yes. SMTP server: smtp.company.com, Port: 587, TLS: Yes. 5. Enter your credentials and click Connect.",
                    Tags = new List<string> { "email", "outlook", "configuration", "setup" },
                    VectorEmbedding = new float[] { 0.45f, 0.62f, 0.18f, 0.83f, 0.34f, 0.71f, 0.09f, 0.56f },
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new Article
                {
                    Title = "Printer Setup and Common Issues",
                    Content = "To connect to a network printer: 1. Open Settings > Devices > Printers. 2. Click 'Add Printer'. 3. Select the printer from the list or enter its IP address. Common issues: Printer shows offline - restart the print spooler service. Paper jam - open the front cover and carefully remove stuck paper. Low quality prints - run the printer head cleaning utility.",
                    Tags = new List<string> { "printer", "hardware", "setup", "troubleshooting" },
                    VectorEmbedding = new float[] { 0.67f, 0.29f, 0.84f, 0.11f, 0.73f, 0.42f, 0.58f, 0.36f },
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Article
                {
                    Title = "Two-Factor Authentication Setup Guide",
                    Content = "Enable two-factor authentication (2FA) to secure your account: 1. Go to Security Settings in your profile. 2. Click 'Enable 2FA'. 3. Download an authenticator app (Google Authenticator or Microsoft Authenticator). 4. Scan the QR code with the app. 5. Enter the 6-digit code to verify. 6. Save backup codes in a secure location. 2FA adds an extra layer of security beyond your password.",
                    Tags = new List<string> { "security", "2fa", "authentication", "account" },
                    VectorEmbedding = new float[] { 0.23f, 0.91f, 0.47f, 0.58f, 0.14f, 0.82f, 0.35f, 0.69f },
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Article
                {
                    Title = "Software Installation and Licensing",
                    Content = "To request new software: 1. Submit a ticket through the helpdesk portal. 2. Specify the software name and version. 3. Provide business justification. 4. Wait for manager approval. 5. IT will deploy the software remotely within 48 hours. For license issues: contact software-licenses@company.com. Supported software includes Microsoft Office, Adobe Creative Suite, Slack, and Zoom.",
                    Tags = new List<string> { "software", "installation", "licensing", "request" },
                    VectorEmbedding = new float[] { 0.56f, 0.38f, 0.75f, 0.22f, 0.89f, 0.14f, 0.63f, 0.47f },
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };

            foreach (var article in articles)
                await session3.StoreAsync(article);

            await session3.SaveChangesAsync();
        }
    }
}