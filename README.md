# <img src="https://i.imgur.com/dAVgTNy.png" alt="MyPortal" width="500"/>

**MyPortal** is a modern, open-source, cloud-first **School Information Management System (MIS)** designed for real school workflows — not vendor lock-in.

It brings together **staff, students, parents, and administrators** into a single, coherent platform covering attendance, assessments, behaviour, communication, and core school operations.

MyPortal is built to be **transparent, extensible, and self-hostable**, giving schools full control over their data and infrastructure.

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)

---

## 🎯 Project Goals

MyPortal exists to challenge the status quo of opaque, expensive, legacy MIS platforms.

Its core goals are:

- Open, inspectable source code  
- Modern web architecture  
- Support for UK school workflows  
- Strong permissions and safeguarding controls  
- Freedom to self-host or deploy in the cloud  
- A system that schools *own*, not rent  

---

## 🧩 Core Modules & Features

MyPortal is built as a modular MIS, with each area designed to work independently while integrating cleanly with the rest of the system.

---

### 🕒 Attendance

Comprehensive attendance tracking designed around real school operations.

- Daily, session, and lesson registers  
- Configurable registration periods  
- Late marks, authorised/unauthorised absence, codes  
- Lesson-by-lesson attendance  
- Attendance summaries and analytics  
- Trend tracking over time (student, group, cohort)  
- Persistent absence monitoring  
- Exportable reports for statutory returns  
- Integration with behaviour and safeguarding workflows  

---

### ⚖️ Behaviour

A structured, auditable behaviour management system.

- Behaviour incidents with categorisation and severity  
- Positive behaviour and achievement tracking  
- Detentions (lunch, after-school, internal)  
- Exclusions (internal, fixed-term, permanent)  
- Report cards and monitoring periods  
- Staff follow-up actions and outcomes  
- Chronological behaviour timelines per student  
- Analysis by student, group, year, or behaviour type  

---

### 📝 Exams

Support for both internal and external examination workflows.

- Exam basedata management  
- Result embargo handling  
- Internal assessment results  
- External exam board results  
- Multiple exam series support  
- Domestic exams and formal qualifications  
- Grade boundaries and mappings  
- Secure access controls for sensitive data  
- Results analysis and exports  

---

### 📊 Assessment

Flexible assessment modelling that adapts to different school philosophies.

- Marksheets and assessment entry  
- Result sets and grade sets  
- Aspects and assessment frameworks  
- Custom grading scales (numeric, letter, banded)  
- Assessment windows and cycles  
- Teacher and departmental ownership  
- Longitudinal progress tracking  
- Integration with reporting and analytics  

---

### 📄 Profiles & Reporting

Powerful reporting without rigid templates.

- End-of-year and interim reports  
- Comment banks (departmental and school-wide)  
- Teacher-written comments  
- Structured and free-text inputs  
- Report builder with configurable sections  
- Per-student report history  
- Export to PDF and other formats  

---

### 📚 Curriculum & Timetabling

Core academic structure management.

- Subjects and courses  
- Curriculum pathways  
- Teaching groups and classes  
- Timetabling data model  
- Room and resource allocation  
- Staff deployment  
- Cover and absence handling  
- Academic year and term structures  

---

### 🎓 Student Management

A complete, centralised student record.

- Personal and contact details  
- Guardian and contact relationships  
- Curriculum group memberships  
- Medical information and alerts  
- Safeguarding flags  
- Behaviour and attendance summaries  
- Historical data retention  
- Secure access based on role  

---

### 👩‍🏫 Staff Management

Staff records designed for operational reality.

- Personal and professional details  
- Roles and responsibilities  
- Service terms and contracts  
- Training and CPD records  
- Qualifications and compliance tracking  
- Department and pastoral assignments  
- Historical employment data  

---

### 🧩 SEND

Support for Special Educational Needs and Disabilities.

- SEND status tracking  
- Provisions and support strategies  
- IEPs and support plans  
- Review schedules and outcomes  
- Staff visibility controls  
- Integration with attendance, behaviour, and assessment  
- Chronological SEND history  

---

### 📁 Documents & Attachments

Secure, auditable document management.

- File storage and attachments  
- Linked to students, staff, or records  
- Versioning and auditing  
- Access control by role  
- Cloud or self-hosted storage options  
- Activity logging  

---

### 📨 Admissions

Tools to support the admissions pipeline.

- Enquiries and applications  
- Applicant records  
- Status tracking  
- Conversion to enrolled students  
- Historical admissions data  

---

### 🏫 School Configuration

School-wide structure and operational settings.

- School details and metadata  
- Pastoral structure (houses, forms, year groups)  
- Academic structure (years, terms, cycles)  
- End-of-year routines and rollover  
- Bulletins and announcements  
- Calendar and key dates  

---

### ⚙️ System Administration

Fine-grained control and governance.

- User accounts  
- Roles and permissions  
- Feature access controls  
- Audit logging  
- Security and authentication settings  
- Integration points and APIs  
- System-wide configuration  

---

## 🧠 Design Philosophy

MyPortal prioritises:

- Data integrity over convenience  
- Explicit modelling over hidden logic  
- Auditable decisions and changes  
- Real school workflows over theoretical ones  

Each module is designed to stand on its own while contributing to a coherent whole.


---

## 🏗️ Tech Stack

- **Backend:** ASP.NET Core Web API  
- **Frontend:** Angular  
- **Database:** SQL Server (with support for additional providers planned)  
- **Authentication:** OAuth 2.0 / OpenID Connect  
- **Architecture:** Modular, API-first design  

---

## 🚀 Getting Started

Setup and deployment documentation is **coming soon**.

In the meantime:
- Watch this repository for updates
- Follow the project on Twitter for progress and previews

👉 **https://twitter.com/MyPortalEDU**

---

## 📢 Project Status

MyPortal is under **active development**.

Core services and foundational modules are in place, with additional functionality being built iteratively. Stability, data integrity, and correctness take priority over rapid feature churn.

This is a long-term project, not a throwaway prototype.

---

## 🤝 Contributing

Contributions are welcome from developers, educators, and anyone interested in improving school systems.

Good areas to contribute include:
- Backend services
- Frontend UI/UX
- Documentation
- UK education domain knowledge
- Testing and deployment tooling

Open an issue or discussion to get started.

---

## 📄 License

MyPortal is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**.

This ensures:
- The software remains free and open
- Improvements must be shared if deployed publicly
- Schools retain freedom and transparency

See the [LICENSE](https://www.gnu.org/licenses/agpl-3.0) file for details.
