# GitHub Issues for FinTrack

This document provides information about creating GitHub issues for all incomplete user stories and tasks in the FinTrack project.

## Summary

A total of **84 GitHub issues** need to be created:

### User Stories
- **8** Import module user stories (US-I1 to US-I8)
- **9** Rules Engine module user stories (US-R1 to US-R9)
- **10** Transactions module user stories (US-T1 to US-T10)
- **8** Dashboard module user stories (US-D1 to US-D8)
- **7** NLQ module user stories (US-N1 to US-N7)

### Implementation Tasks
- **11** Phase 2 tasks (Import & Rules)
- **11** Phase 3 tasks (Dashboard)
- **10** Phase 4 tasks (NLQ & Export)
- **10** Phase 5 tasks (Polish)

## Creating Issues

### Option 1: Using the Script (Automated)

A bash script has been created to automatically create all issues.

#### Prerequisites
1. GitHub CLI (`gh`) must be installed
2. You must be authenticated with GitHub

#### Steps

```bash
# 1. Authenticate with GitHub (if not already authenticated)
gh auth login

# 2. Navigate to the project directory
cd /home/user/fintrack

# 3. Run the script
./create-github-issues.sh
```

The script will create all 84 issues with:
- Clear titles with IDs (e.g., `[US-I1]`, `[Task 2.1]`)
- Detailed descriptions with acceptance criteria
- Appropriate labels for filtering
- Links to related documentation

### Option 2: Manual Creation

If you prefer to create issues manually or in batches, refer to the module documentation files:

- `.claude/modules/import.md` - Import module user stories
- `.claude/modules/rules-engine.md` - Rules Engine user stories
- `.claude/modules/transactions.md` - Transactions user stories
- `.claude/modules/dashboard.md` - Dashboard user stories
- `.claude/modules/nlq.md` - NLQ user stories
- `prompts/phase-2-import-rules.md` - Phase 2 tasks
- `prompts/phase-3-dashboard.md` - Phase 3 tasks
- `prompts/phase-4-nlq-export.md` - Phase 4 tasks
- `prompts/phase-5-polish.md` - Phase 5 tasks

## Issue Labels

The issues will be tagged with the following labels for easy filtering:

### Type Labels
- `user-story` - User stories from module documentation
- `task` - Implementation tasks from phase prompts

### Phase Labels
- `phase-2` - Import & Rules phase
- `phase-3` - Dashboard phase
- `phase-4` - NLQ & Export phase
- `phase-5` - Polish phase

### Module Labels
- `import` - Import module
- `rules-engine` - Rules Engine module
- `transactions` - Transactions module
- `dashboard` - Dashboard module
- `nlq` - Natural Language Query module
- `export` - Export/Import functionality

### Technical Labels
- `backend` - Backend implementation
- `frontend` - Frontend implementation
- `database` - Database changes
- `llm` - LLM integration
- `security` - Security related
- `performance` - Performance optimization
- `testing` - Testing related
- `quality` - Quality improvements
- `documentation` - Documentation
- `a11y` - Accessibility
- `enhancement` - Enhancement/optional feature

## Issue Format

### User Stories

User stories follow this format:

```markdown
**As a** [role]
**I want to** [action]
**So that** [benefit]

## Acceptance Criteria
- [ ] Criterion 1
- [ ] Criterion 2
- [ ] ...

## Related
- Module: [module name] ([path])
- Phase: [phase number and name]
- Task: [related task if applicable]
```

### Tasks

Implementation tasks follow this format:

```markdown
## Description
[Task description]

## Requirements
- Requirement 1
- Requirement 2
- ...

## Deliverables
- [ ] Deliverable 1
- [ ] Deliverable 2
- [ ] ...

## Related
- Phase: [phase number and name]
- Prompt: [path to phase prompt file]
```

## Tracking Progress

Once issues are created, you can track progress using:

1. **GitHub Projects** - Create a project board to visualize progress
2. **Milestones** - Group issues by phase using milestones
3. **Labels** - Filter issues by phase, module, or technical area
4. **Issue Templates** - Use the generated issues as templates for future work

## Next Steps

After creating the issues:

1. **Prioritize** - Determine which issues to work on first
2. **Assign** - Assign issues to team members (if working in a team)
3. **Estimate** - Add time estimates or story points if desired
4. **Link** - Link related issues (e.g., tasks that depend on others)
5. **Track** - Update issue status as work progresses

## Completion Criteria

Refer to the phase prompt files for completion criteria:

- **Phase 2** - `prompts/phase-2-import-rules.md` (11 checkboxes)
- **Phase 3** - `prompts/phase-3-dashboard.md` (11 checkboxes)
- **Phase 4** - `prompts/phase-4-nlq-export.md` (11 checkboxes)
- **Phase 5** - `prompts/phase-5-polish.md` (11 checkboxes)

## Resources

- Project Documentation: `docs/SPEC.md`, `docs/ARCHITECTURE.md`
- Module Documentation: `.claude/modules/*.md`
- Phase Prompts: `prompts/phase-*.md`
- Project Overview: `.claude/CLAUDE.md`
