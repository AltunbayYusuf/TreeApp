export type ConditionalData = { trigger: string; ai: boolean; question: string; };
export type QuestionData = { title: string; type: string; answers: string[]; min: string; max: string; conditionals: ConditionalData[]; };
export type SectionData = { title: string; questions: QuestionData[]; };