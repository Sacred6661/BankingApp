export const passwordRules = {
  minLength: {
    check: (v: string) => v.length >= 8,
    label: "At least 8 characters",
  },
  hasNumber: {
    check: (v: string) => /\d/.test(v),
    label: "Contains a number",
  },
  hasUppercase: {
    check: (v: string) => /[A-Z]/.test(v),
    label: "Contains uppercase letter",
  },
  hasSpecial: {
    check: (v: string) => /[^A-Za-z0-9]/.test(v),
    label: "Contains special character",
  },
};

export type PasswordRuleKey = keyof typeof passwordRules;

export function validatePassword(password: string) {
  return Object.fromEntries(
    Object.entries(passwordRules).map(([key, rule]) => [
      key,
      rule.check(password),
    ]),
  ) as Record<PasswordRuleKey, boolean>;
}

export function isPasswordValid(password: string) {
  return Object.values(validatePassword(password)).every(Boolean);
}
