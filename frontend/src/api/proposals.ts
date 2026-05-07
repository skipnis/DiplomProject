import { apiGet, apiPatch, apiPost, apiPostForm } from './client';
import type {
  CreateProposalRequest,
  IncomingProposalDto,
  OutgoingProposalDto,
  PagedResponse,
  ProposalDetailDto,
  ReactToProposalRequest,
} from '../types';

export function getIncomingProposals(
  page = 1,
  pageSize = 20,
): Promise<PagedResponse<IncomingProposalDto>> {
  return apiGet<PagedResponse<IncomingProposalDto>>(
    `/proposals/incoming?page=${page}&pageSize=${pageSize}`,
  );
}

export function getOutgoingProposals(
  page = 1,
  pageSize = 20,
): Promise<PagedResponse<OutgoingProposalDto>> {
  return apiGet<PagedResponse<OutgoingProposalDto>>(
    `/proposals/outgoing?page=${page}&pageSize=${pageSize}`,
  );
}

export function getProposalDetail(id: string): Promise<ProposalDetailDto> {
  return apiGet<ProposalDetailDto>(`/proposals/${id}`);
}

export function createProposal(data: CreateProposalRequest): Promise<string> {
  return apiPost<string>('/proposals', data);
}

export function reactToProposal(id: string, data: ReactToProposalRequest): Promise<void> {
  return apiPatch<void>(`/proposals/${id}/react`, data);
}

export function uploadProposalImage(id: string, file: File): Promise<{ imagePath: string }> {
  const formData = new FormData();
  formData.append('file', file);
  return apiPostForm<{ imagePath: string }>(`/proposals/${id}/image`, formData);
}
