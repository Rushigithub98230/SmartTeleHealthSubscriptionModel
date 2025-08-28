# Superadmin Master Management & Platform Control

This README tracks all superadmin-managed features and platform-wide settings that must be implemented for robust, compliant, and scalable healthcare operations. Use this as a living checklist to monitor what has been developed and what remains.

---

## 1. Appointment Types & Durations
- **Superadmin:** Define and manage standard appointment types (e.g., 15-min, 30-min, follow-up, group session) and their durations for uniform scheduling and billing.
- **Provider:** Specify session duration when creating subscription plans.

## 2. Provider Availability & Teleconsultation Scheduling
### Provider Availability Management
- Providers can define and manage their working hours and availability (days, time slots) through a dedicated interface. This schedule will be used to allow patients to book teleconsultations as per their active plan.

### Superadmin-Controlled Consultation Duration
- Superadmin has the authority to define the standard duration for each teleconsultation within a subscription plan. For example, if a plan includes 3 teleconsultations, the superadmin can set each session to be 15, 20, or 30 minutes — ensuring uniformity and optimized time allocation.

### Patient Booking Experience
- Based on the provider’s availability and the duration set by the superadmin, patients will see available time slots and can easily schedule their next consultation.

### Conflict-Free Scheduling System
- The platform will intelligently handle appointment bookings to avoid overlaps or double-booking, allowing providers to manage their time efficiently.

### Provider Notifications
- Once a patient books a slot, the provider will be notified with the consultation duration and timing, helping them prepare and manage their schedule accordingly.

### Slot Request Flow (When No Matching Slot is Available)
- If no suitable slot is available that matches the teleconsultation duration defined in the patient’s subscription plan (e.g., the plan includes 30-minute sessions but the provider has only 15 or 60-minute slots available), then the patient will only see matching-duration slots while trying to book.
- If no such matching slot is available, the patient can send a slot request to the assigned provider.
- The provider will review the patient’s subscription plan and the required consultation duration, and can then create a new slot that fits the plan’s requirements.
- Once created, this slot will be automatically reserved for the patient and won’t be visible to others (or will have a tag such as "reserved").

---

## 3. Global Platform Settings
- **Superadmin:** Manage default time zone, currency, language options, branding, and other global settings for platform consistency.

## 4. Payment & Payout Settings
- **Superadmin:** Configure payout frequency (weekly/monthly), minimum payout threshold, supported payout methods, and update as business needs change.
- **Provider:** Providers should see the status of each payout (pending, processing, paid, on hold, etc.).

## 5. Compliance & Legal Documents
- **Superadmin:** Manage and update Terms of Service, Privacy Policy, Consent Forms, HIPAA/PCI compliance docs. Track provider/patient acceptance.

## 6. Audit & Logging Policies
- **Superadmin:** Configure, review, and access all system logs. Admin UI for easy log review and compliance monitoring.
- **Audit Trail:** Log all critical actions (data changes, logins, payouts, etc.) and make logs easily searchable for compliance. Keep a full audit trail of all fee proposals, approvals, and changes (who changed what, when, and why). Use soft deletes for master data and other data to prevent accidental loss and allow for recovery.

## 7. User Role & Permission Management
- **Superadmin:** Create new roles, assign permissions, and control access throughout the system.

## 8. Discounts, Coupons, and Promotions
- **Superadmin:** Manage all marketing and pricing incentives (promo codes, referral bonuses, trial periods).

## 9. Communication Settings
- **Superadmin:** Control notification channels (email, SMS, push), frequency, and all templates. Edit and update templates as needed.

## 10. Provider Review & Rating Moderation
- **Superadmin:** Moderate, hide, or respond to provider reviews/ratings. Tools to ensure quality and fairness.

## 11. Blacklist/Whitelist & User Management
- **Superadmin:** Manage banned users/providers, approved partners, restricted regions for security and compliance.

## 12. Data Export & Backup Policies
- **Superadmin:** Control who can export data, backup frequency, and retention. Monitor all data export/backup activities.

---

## 13. Plan Usage Tracking & Consultation Management
- **Plan Usage Tracking:** Patients and providers should see how many consults are used/remaining in the current plan period.
- **Plan Upgrade/Downgrade Logic:** Handle proration, carryover, or reset of unused consults when a patient changes plans.
- **Consultation Expiry Reminders:** Notify patients if they have unused consults about to expire.

## 14. Provider Onboarding Analytics
- **Admin Dashboard:** Show onboarding funnel (e.g., how many started, completed, dropped off, and why).

---

## Implementation Tracking Table

| Feature/Area                        | Status (Not Started/In Progress/Done) | Notes/Links |
|-------------------------------------|:-------------------------------------:|-------------|
| Appointment Types & Durations       | Not Started                           |             |
| Provider Availability & Scheduling  | Not Started                           |             |
| Global Platform Settings            | Not Started                           |             |
| Payment & Payout Settings           | Not Started                           |             |
| Compliance & Legal Documents        | Not Started                           |             |
| Audit & Logging Policies            | Not Started                           |             |
| User Role & Permission Management   | Not Started                           |             |
| Discounts, Coupons, Promotions      | Not Started                           |             |
| Communication Settings              | Not Started                           |             |
| Provider Review & Rating Moderation | Not Started                           |             |
| Blacklist/Whitelist & User Mgmt     | Not Started                           |             |
| Data Export & Backup Policies       | Not Started                           |             |
| Plan Usage Tracking & Consult Mgmt  | Not Started                           |             |
| Provider Onboarding Analytics       | Not Started                           |             |

---

**Refer to this README throughout development to track progress and ensure all superadmin-managed features are implemented to the highest standard. Update the status and notes as you build each feature.** 