import { useState } from 'react';
import { Plus, Trash2, Edit2, Play, AlertCircle, CheckCircle } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { useActiveProfile } from '../../hooks/use-active-profile';
import { useRules, useCreateRule, useUpdateRule, useDeleteRule, useTestRules, useApplyRules } from '../../hooks/use-rules';
import type { Rule, CreateRuleRequest, UpdateRuleRequest, TestRulesRequest } from '../../lib/types';

const RULE_TEMPLATES = [
  {
    name: 'Amazon Shopping',
    toml: `name = "Amazon Shopping"
priority = 10
category = "Shopping"

[match.description]
contains = ["AMAZON", "AMZN"]`,
  },
  {
    name: 'Netflix Subscription',
    toml: `name = "Netflix"
priority = 20
category = "Subscriptions"
subcategory = "Streaming"

[match.description]
contains = ["NETFLIX"]`,
  },
  {
    name: 'Uber/Lyft Rideshare',
    toml: `name = "Rideshare"
priority = 30
category = "Transport"
subcategory = "Rideshare"

[match.description]
contains = ["UBER", "LYFT"]`,
  },
  {
    name: 'Grocery Stores',
    toml: `name = "Groceries"
priority = 40
category = "Food"
subcategory = "Groceries"

[match.description]
contains = ["WALMART", "TARGET", "COSTCO", "WHOLE FOODS", "TRADER JOE"]`,
  },
  {
    name: 'Restaurant (by amount)',
    toml: `name = "Dining Out"
priority = 50
category = "Food"
subcategory = "Restaurants"

[match.description]
contains = ["RESTAURANT", "CAFE", "PIZZA", "BURGER", "DOORDASH", "GRUBHUB"]

[match.amount]
range = [-200, -5]`,
  },
  {
    name: 'Salary Income',
    toml: `name = "Salary"
priority = 5
category = "Income"
subcategory = "Salary"

[match.amount]
greater_than = 1000`,
  },
];

const DEFAULT_RULE_TOML = RULE_TEMPLATES[0].toml;

export function RulesPage() {
  const { activeProfileId } = useActiveProfile();
  const { data: rules, isLoading } = useRules(activeProfileId ?? undefined);

  const [isEditing, setIsEditing] = useState(false);
  const [editingRule, setEditingRule] = useState<Rule | null>(null);
  const [formData, setFormData] = useState<CreateRuleRequest>({
    name: '',
    priority: 0,
    ruleToml: DEFAULT_RULE_TOML,
    isActive: true,
  });

  const [testInput, setTestInput] = useState<TestRulesRequest>({
    description: '',
    amount: 0,
    date: new Date().toISOString().split('T')[0],
  });
  const [testResult, setTestResult] = useState<{ matched: boolean; ruleName?: string; category?: string } | null>(null);

  const createMutation = useCreateRule();
  const updateMutation = useUpdateRule();
  const deleteMutation = useDeleteRule();
  const testMutation = useTestRules();
  const applyMutation = useApplyRules();

  const handleCreateNew = () => {
    setEditingRule(null);
    setFormData({
      name: 'New Rule',
      priority: (rules?.length ?? 0) * 10 + 10,
      ruleToml: DEFAULT_RULE_TOML,
      isActive: true,
    });
    setIsEditing(true);
  };

  const handleEdit = (rule: Rule) => {
    setEditingRule(rule);
    setFormData({
      name: rule.name,
      priority: rule.priority,
      ruleToml: rule.ruleToml,
      isActive: rule.isActive,
    });
    setIsEditing(true);
  };

  const handleSave = async () => {
    if (!activeProfileId) return;

    try {
      if (editingRule) {
        await updateMutation.mutateAsync({
          id: editingRule.id,
          data: formData as UpdateRuleRequest,
        });
      } else {
        await createMutation.mutateAsync({
          profileId: activeProfileId,
          data: formData,
        });
      }
      setIsEditing(false);
      setEditingRule(null);
    } catch (error) {
      console.error('Failed to save rule:', error);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this rule?')) return;
    await deleteMutation.mutateAsync(id);
  };

  const handleTest = async () => {
    if (!activeProfileId) return;

    try {
      const result = await testMutation.mutateAsync({
        profileId: activeProfileId,
        data: testInput,
      });
      setTestResult({
        matched: !!result.matchedRuleName,
        ruleName: result.matchedRuleName ?? undefined,
        category: result.category ?? undefined,
      });
    } catch (error) {
      console.error('Test failed:', error);
    }
  };

  const handleApplyRules = async () => {
    if (!activeProfileId) return;

    try {
      const result = await applyMutation.mutateAsync({
        profileId: activeProfileId,
        onlyUncategorized: true,
      });
      alert(`Updated ${result.transactionsUpdated} transactions`);
    } catch (error) {
      console.error('Apply rules failed:', error);
    }
  };

  if (!activeProfileId) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500">Please select a profile first</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold">Categorization Rules</h1>
          <p className="text-gray-500">Define rules to automatically categorize transactions</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={handleApplyRules} disabled={applyMutation.isPending}>
            <Play className="h-4 w-4 mr-2" />
            Apply Rules
          </Button>
          <Button onClick={handleCreateNew}>
            <Plus className="h-4 w-4 mr-2" />
            New Rule
          </Button>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        {/* Rules List */}
        <Card>
          <CardHeader>
            <CardTitle>Active Rules</CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <p className="text-gray-500">Loading...</p>
            ) : rules?.length === 0 ? (
              <p className="text-gray-500">No rules yet. Create your first rule!</p>
            ) : (
              <div className="space-y-3">
                {rules?.map(rule => (
                  <div
                    key={rule.id}
                    className={`p-3 border rounded-lg ${rule.isActive ? '' : 'opacity-50'}`}
                  >
                    <div className="flex justify-between items-start">
                      <div>
                        <div className="font-medium">{rule.name}</div>
                        <div className="text-sm text-gray-500">Priority: {rule.priority}</div>
                      </div>
                      <div className="flex gap-1">
                        <Button variant="ghost" size="sm" onClick={() => handleEdit(rule)}>
                          <Edit2 className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleDelete(rule.id)}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                    <pre className="mt-2 p-2 bg-gray-50 rounded text-xs overflow-x-auto">
                      {rule.ruleToml}
                    </pre>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Rule Editor / Test Panel */}
        <div className="space-y-6">
          {isEditing ? (
            <Card>
              <CardHeader>
                <CardTitle>{editingRule ? 'Edit Rule' : 'New Rule'}</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <Label htmlFor="name">Name</Label>
                  <Input
                    id="name"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  />
                </div>
                <div>
                  <Label htmlFor="priority">Priority (lower = first)</Label>
                  <Input
                    id="priority"
                    type="number"
                    value={formData.priority}
                    onChange={(e) => setFormData({ ...formData, priority: parseInt(e.target.value) || 0 })}
                  />
                </div>
                {!editingRule && (
                  <div>
                    <Label htmlFor="template">Start from Template</Label>
                    <select
                      id="template"
                      className="w-full h-10 px-3 border rounded text-sm"
                      onChange={(e) => {
                        const template = RULE_TEMPLATES[parseInt(e.target.value)];
                        if (template) {
                          setFormData({
                            ...formData,
                            name: template.name,
                            ruleToml: template.toml,
                          });
                        }
                      }}
                    >
                      <option value="">Select a template...</option>
                      {RULE_TEMPLATES.map((t, i) => (
                        <option key={i} value={i}>{t.name}</option>
                      ))}
                    </select>
                  </div>
                )}
                <div>
                  <Label htmlFor="toml">Rule TOML</Label>
                  <textarea
                    id="toml"
                    className="w-full h-48 p-2 border rounded font-mono text-sm"
                    value={formData.ruleToml}
                    onChange={(e) => setFormData({ ...formData, ruleToml: e.target.value })}
                  />
                </div>
                <div className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    id="isActive"
                    checked={formData.isActive}
                    onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                  />
                  <Label htmlFor="isActive">Active</Label>
                </div>
                <div className="flex gap-2">
                  <Button variant="outline" onClick={() => setIsEditing(false)}>
                    Cancel
                  </Button>
                  <Button onClick={handleSave} disabled={createMutation.isPending || updateMutation.isPending}>
                    Save Rule
                  </Button>
                </div>
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardHeader>
                <CardTitle>Test Rules</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <Label htmlFor="testDesc">Transaction Description</Label>
                  <Input
                    id="testDesc"
                    placeholder="e.g., AMAZON.COM PURCHASE"
                    value={testInput.description}
                    onChange={(e) => setTestInput({ ...testInput, description: e.target.value })}
                  />
                </div>
                <div>
                  <Label htmlFor="testAmount">Amount</Label>
                  <Input
                    id="testAmount"
                    type="number"
                    step="0.01"
                    value={testInput.amount}
                    onChange={(e) => setTestInput({ ...testInput, amount: parseFloat(e.target.value) || 0 })}
                  />
                </div>
                <div>
                  <Label htmlFor="testDate">Date</Label>
                  <Input
                    id="testDate"
                    type="date"
                    value={testInput.date}
                    onChange={(e) => setTestInput({ ...testInput, date: e.target.value })}
                  />
                </div>
                <Button onClick={handleTest} disabled={testMutation.isPending}>
                  Test Rules
                </Button>

                {testResult && (
                  <div className={`p-3 rounded-lg flex items-start gap-2 ${
                    testResult.matched ? 'bg-green-50' : 'bg-yellow-50'
                  }`}>
                    {testResult.matched ? (
                      <>
                        <CheckCircle className="h-5 w-5 text-green-600 flex-shrink-0" />
                        <div>
                          <div className="font-medium text-green-800">Match found!</div>
                          <div className="text-sm text-green-700">
                            Rule: {testResult.ruleName}
                            {testResult.category && <> | Category: {testResult.category}</>}
                          </div>
                        </div>
                      </>
                    ) : (
                      <>
                        <AlertCircle className="h-5 w-5 text-yellow-600 flex-shrink-0" />
                        <div className="text-yellow-800">No matching rule found</div>
                      </>
                    )}
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          {/* TOML Reference */}
          <Card>
            <CardHeader>
              <CardTitle>TOML Reference</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-sm text-gray-600 space-y-2">
                <p><strong>Description matchers:</strong></p>
                <ul className="list-disc list-inside ml-2 space-y-1">
                  <li><code className="bg-gray-100 px-1 rounded">contains = ["text"]</code> - Substring match (OR)</li>
                  <li><code className="bg-gray-100 px-1 rounded">equals = "text"</code> - Exact match</li>
                  <li><code className="bg-gray-100 px-1 rounded">starts_with = "text"</code> - Prefix</li>
                  <li><code className="bg-gray-100 px-1 rounded">ends_with = "text"</code> - Suffix</li>
                  <li><code className="bg-gray-100 px-1 rounded">regex = "pattern"</code> - Regex</li>
                </ul>
                <p className="mt-3"><strong>Amount matchers:</strong></p>
                <ul className="list-disc list-inside ml-2 space-y-1">
                  <li><code className="bg-gray-100 px-1 rounded">equals = -50.00</code></li>
                  <li><code className="bg-gray-100 px-1 rounded">greater_than = 100</code></li>
                  <li><code className="bg-gray-100 px-1 rounded">less_than = -500</code></li>
                  <li><code className="bg-gray-100 px-1 rounded">range = [-100, -10]</code></li>
                </ul>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
